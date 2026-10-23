param([Parameter(Mandatory=$true)][string]$RepoRoot,[Parameter(Mandatory=$true)][string]$ProfileDirectory,[Parameter(Mandatory=$true)][string]$Dll,[ValidateSet('Fixed','RingtoneLength')][string]$Mode)
$ErrorActionPreference='Stop'
function Assert-Closed {
 if(Get-Process -Name 'Lethal Company' -ErrorAction SilentlyContinue){throw 'Bitte Lethal Company vor der Testinstallation schliessen.'}
}
function Assert-Child([string]$Candidate,[string]$Parent){
 $resolved=[IO.Path]::GetFullPath($Candidate)
 $boundary=[IO.Path]::GetFullPath($Parent).TrimEnd('\')+'\'
 if(-not $resolved.StartsWith($boundary,[StringComparison]::OrdinalIgnoreCase)){throw "Ziel liegt ausserhalb des erlaubten Profilbereichs: $resolved"}
 return $resolved
}
Assert-Closed
if(-not(Test-Path -LiteralPath (Join-Path $ProfileDirectory 'mods.yml'))){throw 'Kein gueltiges r2modman-Profil ausgewaehlt.'}
if(-not $Mode){
 Write-Host 'Testmodus: 1 = feste 45 Sekunden, 2 = individuelle Klingeltonlaenge'
 do{$answer=Read-Host 'Wahl [1/2], Enter = 1'}while($answer -notin @('','1','2'))
 $Mode=if($answer -eq '2'){'RingtoneLength'}else{'Fixed'}
}
$plugins=Join-Path $ProfileDirectory 'BepInEx\plugins'
$configRoot=Join-Path $ProfileDirectory 'BepInEx\config'
$destination=Assert-Child (Join-Path $plugins 'LethalPhonesCallTimeout\LethalPhones.CallTimeout.dll') $plugins
$legacy=Assert-Child (Join-Path $plugins 'PrivatePhoneTimeout\PrivatePhoneTimeout.dll') $plugins
$cfg=Assert-Child (Join-Path $configRoot 'lethalphones.calltimeout.cfg') $configRoot
foreach($folder in @((Split-Path $destination),(Split-Path $legacy),$configRoot)){
 if((Test-Path -LiteralPath $folder) -and ((Get-Item -LiteralPath $folder).Attributes -band [IO.FileAttributes]::ReparsePoint)){throw "Verknuepften Zielordner bitte vorher pruefen: $folder"}
}
$backup=Join-Path $RepoRoot ('artifacts\backups\'+(Get-Date -Format 'yyyy-MM-dd_HH-mm-ss-ffff'))
New-Item -ItemType Directory -Path $backup -Force | Out-Null
$existed=Test-Path -LiteralPath $destination
$cfgExisted=Test-Path -LiteralPath $cfg
if($existed){Copy-Item -LiteralPath $destination -Destination (Join-Path $backup 'LethalPhones.CallTimeout.dll')}
if($cfgExisted){Copy-Item -LiteralPath $cfg -Destination (Join-Path $backup 'lethalphones.calltimeout.cfg')}
$legacyPresent=Test-Path -LiteralPath $legacy
if($legacyPresent){
 $marker=Join-Path (Split-Path $legacy) '.private-phone-timeout-managed.json'
 if(-not(Test-Path -LiteralPath $marker)){throw 'Alter privater Fix ohne Verwaltungsdatei gefunden. Bitte vor der Installation manuell pruefen/entfernen.'}
 $info=Get-Content -LiteralPath $marker -Raw -Encoding UTF8 | ConvertFrom-Json
 if($info.Werkzeug -ne 'PrivatePhoneTimeout'){throw 'Unbekannter alter Fixordner. Keine Datei wurde entfernt.'}
 Copy-Item -LiteralPath $legacy -Destination (Join-Path $backup 'PrivatePhoneTimeout.dll')
}
$disabled=$legacy+'.disabled'
if($legacyPresent -and (Test-Path -LiteralPath $disabled)){throw 'Es existiert bereits eine deaktivierte alte Fix-DLL. Bitte diese zuerst pruefen.'}
Assert-Closed
$legacyMoved=$false
try{
 New-Item -ItemType Directory -Path (Split-Path $destination),$configRoot -Force | Out-Null
 Copy-Item -LiteralPath $Dll -Destination $destination -Force
 if($legacyPresent){Move-Item -LiteralPath $legacy -Destination $disabled;$legacyMoved=$true}
 # Set predictable test settings; the previous configuration is backed up.
 if($cfgExisted){
  $text=Get-Content -LiteralPath $cfg -Raw -Encoding UTF8
  if($text -match '(?m)^Mode\s*='){$text=[regex]::Replace($text,'(?m)^Mode\s*=.*$',('Mode = '+$Mode))}
  else{throw 'Vorhandene Konfiguration hat keinen Mode-Eintrag. Bitte sie vor dem Test manuell pruefen.'}
  $text=[regex]::Replace($text,'(?m)^Enabled\s*=.*$','Enabled = true')
  $text=[regex]::Replace($text,'(?m)^FixedSeconds\s*=.*$','FixedSeconds = 45')
 }else{
  $text="[General]`r`nEnabled = true`r`n`r`n[Calls]`r`nMode = $Mode`r`nFixedSeconds = 45`r`nMaximumRingtoneSeconds = 300`r`nRingtoneGraceSeconds = 0`r`n"
 }
 [IO.File]::WriteAllText($cfg,$text,(New-Object Text.UTF8Encoding($false)))
}catch{
 if($existed){Copy-Item -LiteralPath (Join-Path $backup 'LethalPhones.CallTimeout.dll') -Destination $destination -Force}
 elseif(Test-Path -LiteralPath $destination){Remove-Item -LiteralPath $destination}
 if($cfgExisted){Copy-Item -LiteralPath (Join-Path $backup 'lethalphones.calltimeout.cfg') -Destination $cfg -Force}
 elseif(Test-Path -LiteralPath $cfg){Remove-Item -LiteralPath $cfg}
 if($legacyMoved){Move-Item -LiteralPath $disabled -Destination $legacy}
 throw
}
Write-Host "Testmod installiert. Modus: $Mode"
Write-Host "Sicherung: $backup"
Write-Host 'Jetzt das Spiel normal ueber r2modman starten. Es wurden keine Medien oder globalen Einstellungen geaendert.'
