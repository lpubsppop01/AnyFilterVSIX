$scriptDirPath = Split-Path $MyInvocation.MyCommand.Path -Parent
$assemblyInfoPath = Join-Path $scriptDirPath "..\Properties\AssemblyInfo.cs"
$assemblyVersionLine = Get-Content $assemblyInfoPath | Select-String AssemblyVersion | Select-Object -First 1
$assemblyVersion = $assemblyVersionLine -replace '.*AssemblyVersion\("(.*)"\).*', '$1'
$manifestFilePath = Join-Path $scriptDirPath "..\source.extension.vsixmanifest"
$manifestXML = [xml](Get-Content $manifestFilePath)
$identityElement = $manifestXML.SelectSingleNode("//*[local-name(.) = 'Identity']")

#$oldVersion = $identityElement.Version
#$v = [version]$identityElement.Version
#$newVersion = $v.Major.ToString() + "." + $v.Minor.ToString() + "." + ($v.Build + 1).ToString()
#Write-Host "Version: $oldVersion -> $newVersion"

$identityElement.Version = $assemblyVersion
$manifestXML.Save($manifestFilePath)
