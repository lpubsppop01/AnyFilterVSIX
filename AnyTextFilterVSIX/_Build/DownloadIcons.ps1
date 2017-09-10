$here = Split-Path -Parent $MyInvocation.MyCommand.Source
$iconDirPath = Join-Path (Split-Path -Parent $here) "_Icons"
$uriPrefix = "https://raw.githubusercontent.com/Templarian/WindowsIcons/master/WindowsPhone/xaml/"

if (!(Test-Path -LiteralPath $iconDirPath)) {
    mkdir $iconDirPath
}

function Download-XamlIcon($filename) {
    $filePath = (Join-Path $iconDirPath $filename)
    if (Test-Path -LiteralPath $filePath) {
        return
    }
    $uri = $uriPrefix + $filename
    Invoke-WebRequest $uri -OutFile $filePath
}

Download-XamlIcon "appbar.chevron.up.xaml"
Download-XamlIcon "appbar.chevron.down.xaml"
