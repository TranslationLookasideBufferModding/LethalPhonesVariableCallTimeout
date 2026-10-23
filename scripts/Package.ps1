param([Parameter(Mandatory=$true)][string]$RepoRoot,[Parameter(Mandatory=$true)][string]$Dll)
$ErrorActionPreference='Stop'
function Get-BinarySha256([string]$Path){
 $stream=[IO.File]::OpenRead($Path);$algorithm=[Security.Cryptography.SHA256]::Create()
 try{return ([BitConverter]::ToString($algorithm.ComputeHash($stream))).Replace('-','')}
 finally{$stream.Dispose();$algorithm.Dispose()}
}
$readme=Get-Content -LiteralPath (Join-Path $RepoRoot 'README.md') -Raw -Encoding UTF8
if([string]::IsNullOrWhiteSpace($readme) -or $readme.Contains('WRITE_YOUR_README')){throw 'Eigenes README.md fehlt.'}
$proof=Get-Content -LiteralPath (Join-Path $RepoRoot 'artifacts\tests\verification.json') -Raw -Encoding UTF8 | ConvertFrom-Json
if($proof.UnityRuntime -ne 'Passed' -or $proof.ArtifactSHA256 -ne (Get-BinarySha256 $Dll)){throw 'Die DLL passt nicht zum erfolgreichen Laufzeittest.'}
$manifestPath=Join-Path $RepoRoot 'package\manifest.json'
$manifest=Get-Content -LiteralPath $manifestPath -Raw -Encoding UTF8 | ConvertFrom-Json
if($manifest.name -notmatch '^[A-Za-z0-9_]{1,128}$' -or $manifest.version_number -notmatch '^\d+\.\d+\.\d+$' -or $manifest.description.Length -gt 250){throw 'Ungueltiges Thunderstore-Manifest.'}
$assembly=[Reflection.AssemblyName]::GetAssemblyName($Dll)
if($assembly.Version.ToString(3) -ne $manifest.version_number){throw 'Manifest- und DLL-Version weichen ab.'}
$release=Join-Path $RepoRoot 'artifacts\releases'
New-Item -ItemType Directory -Path $release -Force | Out-Null
$destination=Join-Path $release ($manifest.name+'-'+$manifest.version_number+'.zip')
Add-Type -AssemblyName System.IO.Compression.FileSystem
Add-Type -AssemblyName System.IO.Compression
$temporary=$destination+'.tmp'
if(Test-Path -LiteralPath $temporary){Remove-Item -LiteralPath $temporary}
$zip=[IO.Compression.ZipFile]::Open($temporary,[IO.Compression.ZipArchiveMode]::Create)
try{
 $files=@{ 'README.md'=(Join-Path $RepoRoot 'README.md');'manifest.json'=$manifestPath;'icon.png'=(Join-Path $RepoRoot 'package\icon.png');'CHANGELOG.md'=(Join-Path $RepoRoot 'package\CHANGELOG.md');'LICENSE.txt'=(Join-Path $RepoRoot 'LICENSE.txt');'plugins/LethalPhonesCallTimeout/LethalPhones.CallTimeout.dll'=$Dll }
 foreach($name in $files.Keys){[IO.Compression.ZipFileExtensions]::CreateEntryFromFile($zip,$files[$name],$name,[IO.Compression.CompressionLevel]::Optimal) | Out-Null}
 foreach($source in (Get-ChildItem -LiteralPath (Join-Path $RepoRoot 'src') -Filter '*.cs' -File)){[IO.Compression.ZipFileExtensions]::CreateEntryFromFile($zip,$source.FullName,('Source/'+$source.Name),[IO.Compression.CompressionLevel]::Optimal) | Out-Null}
}finally{$zip.Dispose()}
Move-Item -LiteralPath $temporary -Destination $destination -Force
Write-Host "Fertiges Uploadpaket: $destination"
Write-Host 'Es wurde nichts hochgeladen und kein Git-Remote geaendert.'
