param([Parameter(Mandatory=$true)][string]$ProfileDirectory)
$ErrorActionPreference='Stop'
Add-Type -Path (Join-Path $ProfileDirectory 'BepInEx\core\Mono.Cecil.dll')
$lp=Join-Path $ProfileDirectory 'BepInEx\plugins\Scoops-LethalPhones\LethalPhones.dll'
if(-not(Test-Path -LiteralPath $lp)){throw 'LethalPhones.dll fehlt im eingetragenen Profil.'}
$assembly=[Mono.Cecil.AssemblyDefinition]::ReadAssembly($lp)
try{
 $plugin=$assembly.MainModule.GetType('Scoops.Plugin')
 $metadata=$plugin.CustomAttributes | Where-Object {$_.AttributeType.Name -eq 'BepInPlugin'} | Select-Object -First 1
 if($metadata.ConstructorArguments[0].Value -ne 'LethalPhones' -or $metadata.ConstructorArguments[2].Value -ne '1.3.23'){throw 'Die automatische Referenzpruefung ist fuer LethalPhones 1.3.23 vorgesehen.'}
 foreach($name in @('Scoops.misc.PlayerPhone','Scoops.gameobjects.SwitchboardPhone')){
  $type=$assembly.MainModule.GetType($name)
  $state=$type.NestedTypes | Where-Object {$_.Name.StartsWith('<CallTimeoutCoroutine>')} | Select-Object -First 1
  if(-not $state){throw "Coroutine fehlt: $name"}
  foreach($field in @('<>4__this','number')){if(-not($state.Fields | Where-Object Name -eq $field)){throw "Statefeld fehlt: $name $field"}}
  $next=$state.Methods | Where-Object Name -eq 'MoveNext'
  $matches=0
  for($i=0;$i -lt $next.Body.Instructions.Count-1;$i++){
   $instruction=$next.Body.Instructions[$i];$following=$next.Body.Instructions[$i+1]
   if($instruction.OpCode.Code.ToString() -eq 'Ldc_R4' -and [single]$instruction.Operand -eq 15 -and $following.OpCode.Code.ToString() -eq 'Newobj' -and $following.Operand.DeclaringType.FullName -eq 'UnityEngine.WaitForSeconds'){$matches++}
  }
  if($matches -ne 1){throw "Erwarteter 15-Sekunden-Timer fehlt: $name"}
 }
 foreach($entry in @(@('Scoops.misc.PhoneBehavior','phoneNumber'),@('Scoops.misc.PhoneBehavior','phoneRingtoneId'),@('Scoops.service.PhoneNetworkHandler','allPhoneBehaviors'),@('Scoops.customization.CustomizationManager','ringtoneCustomizations'),@('Scoops.customization.CustomizationManager','DEFAULT_RINGTONE'),@('Scoops.service.PhoneAssetManager','switchboardRing'))){
  if(-not($assembly.MainModule.GetType($entry[0]).Fields | Where-Object Name -eq $entry[1])){throw "Erwartetes Laufzeitfeld fehlt: $($entry -join '.')"}
 }
 Write-Host 'INSTALLED_REFERENCES_OK LethalPhones 1.3.23'
}finally{$assembly.Dispose()}
