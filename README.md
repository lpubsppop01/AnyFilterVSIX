# AnyTextFilter Visual Studio Extension
This Visual Studio Extension provides external text filter commands for target ranges,
that passes target text to an arbitrary external program and replace the text by the output of the external program.

## Features
* Settings by "AnyTextFilter Settings..." in Tools menu.
* Registered filter commands will show following the Settings command.
* Some samples are available on filter registration: sed, AWK, Cygwin bash, Mono C# Script

## Download
[AnyTextFilterVSIX.vsix](https://github.com/lpubsppop01/AnyTextFilterVSIX/raw/master/AnyTextFilterVSIX.vsix)

## Requirements
Visual Studio 2013 or later

## Author
[lpubsppop01](https://github.com/lpubsppop01)

## License
[MIT License](https://github.com/lpubsppop01/AnyTextFilterVSIX/raw/master/LICENSE.txt)

This software uses the following Nuget packages:
* [Diff.Match.Patch 2.0.1](https://www.nuget.org/packages/Diff.Match.Patch/)  
  Copyright (c) 2016- pocketberserker  
  Released under the [MIT License](https://github.com/pocketberserker/Diff.Match.Patch/blob/master/LICENSE)
* [FSharp.Core 4.0.0.1](https://www.nuget.org/packages/FSharp.Core/)  
  (c) Microsoft Corporation. All rights reserved.  
  Released under the [Apache License 2.0](https://github.com/fsharp/fsharp/blob/master/LICENSE)
