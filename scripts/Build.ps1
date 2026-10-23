param([ValidateSet('Build','Test','Release','InstallTest')][string]$Task='Build')
$ErrorActionPreference='Stop'
function Get-BinarySha256([string]$Path){
 $stream=[IO.File]::OpenRead($Path);$algorithm=[Security.Cryptography.SHA256]::Create()
 try{return ([BitConverter]::ToString($algorithm.ComputeHash($stream))).Replace('-','')}
 finally{$stream.Dispose();$algorithm.Dispose()}
}
$repoRoot=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$settingsPath=Join-Path $repoRoot 'local.settings.json'
if(-not(Test-Path -LiteralPath $settingsPath)){throw 'Bitte local.settings.example.json als local.settings.json kopieren und die lokalen Pfade eintragen.'}
$local=Get-Content -LiteralPath $settingsPath -Raw -Encoding UTF8 | ConvertFrom-Json
$profile=[IO.Path]::GetFullPath($local.ProfileDirectory)
$game=[IO.Path]::GetFullPath($local.GameDirectory)
$core=Join-Path $profile 'BepInEx\core'
$managed=Join-Path $game 'Lethal Company_Data\Managed'
$artifacts=Join-Path $repoRoot 'artifacts'
$bin=Join-Path $artifacts 'bin'
$reports=Join-Path $artifacts 'tests'
New-Item -ItemType Directory -Path $bin,$reports -Force | Out-Null
$compiler=Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
if(-not(Test-Path -LiteralPath $compiler)){$compiler=Join-Path $env:WINDIR 'Microsoft.NET\Framework\v4.0.30319\csc.exe'}
if(-not(Test-Path -LiteralPath $compiler)){throw 'Der vorhandene Windows-.NET-Framework-C#-Compiler wurde nicht gefunden. Es wurde nichts installiert.'}
$readme=Join-Path $repoRoot 'README.md'
if($Task -eq 'Release'){
 $readmeText=Get-Content -LiteralPath $readme -Raw -Encoding UTF8
 if([string]::IsNullOrWhiteSpace($readmeText) -or $readmeText.Contains('WRITE_YOUR_README')){throw 'Bitte zuerst dein eigenes README.md schreiben. Der Platzhalter darf nicht im Release landen.'}
}
$references=@((Join-Path $core 'BepInEx.dll'),(Join-Path $core '0Harmony.dll'),(Join-Path $managed 'UnityEngine.dll'),(Join-Path $managed 'UnityEngine.CoreModule.dll'),(Join-Path $managed 'UnityEngine.AudioModule.dll'),(Join-Path $managed 'netstandard.dll'))
foreach($reference in $references){if(-not(Test-Path -LiteralPath $reference)){throw "Lokale Referenz fehlt: $reference"}}
$dll=Join-Path $bin 'LethalPhones.CallTimeout.dll'
$compileArgs=@('/nologo','/target:library','/optimize+','/langversion:5',"/out:$dll")
$compileArgs+=@($references | ForEach-Object {"/reference:$_"})
$compileArgs+=@(Get-ChildItem -LiteralPath (Join-Path $repoRoot 'src') -Filter '*.cs' -File | Sort-Object Name | ForEach-Object {$_.FullName})
& $compiler @compileArgs
if($LASTEXITCODE -ne 0){throw 'DLL-Kompilierung fehlgeschlagen.'}
Write-Host "Gebaut: $dll"
if($Task -eq 'Build'){exit 0}

$unit=Join-Path $reports 'CoreTests.exe'
& $compiler /nologo /target:exe /optimize+ /langversion:5 "/out:$unit" (Join-Path $repoRoot 'src\TimeoutPolicy.cs') (Join-Path $repoRoot 'tests\CoreTests.cs')
if($LASTEXITCODE -ne 0){throw 'Kompilierung der Logiktests fehlgeschlagen.'}
$unitOutput=& $unit 2>&1
if($LASTEXITCODE -ne 0){throw ($unitOutput -join "`n")}
$unitOutput | Set-Content -LiteralPath (Join-Path $reports 'core-tests.log')
$unitOutput | ForEach-Object {Write-Host $_}
& (Join-Path $PSScriptRoot 'Verify-References.ps1') -ProfileDirectory $profile

$unityPassed=$false
if($local.UnityEditor -and (Test-Path -LiteralPath $local.UnityEditor)){
 $project=Join-Path $repoRoot 'tests\UnityHarness'
 $unityLog=Join-Path $reports 'unity-runtime.log'
 $arguments=@('-batchmode','-nographics','-quit','-noUpm','-projectPath',('"'+$project+'"'),'-executeMethod','CallTimeoutRuntimeTests.Run','-pluginPath',('"'+$dll+'"'),'-corePath',('"'+$core+'"'),'-logFile',('"'+$unityLog+'"'))
 $process=Start-Process -FilePath $local.UnityEditor -ArgumentList $arguments -PassThru -WindowStyle Hidden
 # Wait only for the editor itself; -Wait also waits for persistent child workers.
 if(-not $process.WaitForExit(180000)){$process.Kill();throw 'Unity-Test hat das Zeitlimit von 180 Sekunden ueberschritten.'}
 $unityText=Get-Content -LiteralPath $unityLog -Raw
 if($process.ExitCode -ne 0 -or -not $unityText.Contains('RUNTIME_TESTS_OK')){
  $unityText -split "`r?`n" | Where-Object {$_ -match 'Exception|error CS|FAIL'} | ForEach-Object {Write-Host $_}
  throw "Unity-Laufzeittest fehlgeschlagen. Siehe $unityLog"
 }
 $unityPassed=$true
 $unityText -split "`r?`n" | Where-Object {$_ -match 'RUNTIME_TESTS_OK'} | ForEach-Object {Write-Host $_}
}else{Write-Warning 'Kein vorhandener Unity-Editor konfiguriert: Logiktests bestanden, Unity-Laufzeittest nicht ausgefuehrt.'}
$proof=[ordered]@{ArtifactSHA256=(Get-BinarySha256 $dll);CoreTests='Passed';InstalledReferences='Passed';UnityRuntime=if($unityPassed){'Passed'}else{'NotRun'}}
$proof | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $reports 'verification.json') -Encoding UTF8
if($Task -eq 'Release'){
 if(-not $unityPassed){throw 'Release abgebrochen: Kein erfolgreicher Unity-Laufzeittest fuer diese DLL.'}
 & (Join-Path $PSScriptRoot 'Package.ps1') -RepoRoot $repoRoot -Dll $dll
}
if($Task -eq 'InstallTest'){
 if(-not $unityPassed){throw 'Testinstallation abgebrochen: Bitte erst den vorhandenen Unity-Editor konfigurieren und die Laufzeittests bestehen.'}
 & (Join-Path $PSScriptRoot 'Install-Test.ps1') -RepoRoot $repoRoot -ProfileDirectory $profile -Dll $dll
}
