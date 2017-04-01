if ($args.Length -gt 0) {
    $configulationName = $args[0]
    if ($configulationName -ine "Publish") {
        return
    }
}

$scriptDirPath = Split-Path $MyInvocation.MyCommand.Path -Parent
$manifestFilePath = Join-Path $scriptDirPath "..\source.extension.vsixmanifest"
$manifestXML = [xml](Get-Content $manifestFilePath)
$identityElement = $manifestXML.SelectSingleNode("//*[local-name(.) = 'Identity']")

$oldVersion = $identityElement.Version
$v = [version]$identityElement.Version
$newVersion = $v.Major.ToString() + "." + $v.Minor.ToString() + "." + ($v.Build + 1).ToString()
Write-Host "Version: $oldVersion -> $newVersion"

$identityElement.Version = $newVersion
$manifestXML.Save($manifestFilePath)
