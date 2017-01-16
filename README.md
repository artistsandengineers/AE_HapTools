#Wat
Tools for working AVI files containing [Hap](https://github.com/Vidvox/hap) video, specifically:
* vvvv-HapTreats - a vvvv node for loading Textures from AVIs.
* hap2dds - CLI tool for extracting Hap encoded frames from AVIs and writing them to .dds files.
* AE_HapAVI - a class library on which the above are based.

#Building
1. `git clone git@github.com:artistsandengineers/AE_HapTools.git
2. `cd vvvv-hap`
3. `git submodule update --init --recursive`
4. For reasons unknown Nuget fails to restore the Snappy packages when AE_HapAVI is built via a VS project reference. Open AE_HapAVI/AE_HapAVI.sln and build it before attempting to build other things. You might need to `Install-Package Snappy.NET` in the Nuget Package Manager Console.

#Limitations
(all of which are resolvable)
* No support for AVI audio.
* No support for HapQ/HapQ Alpha.
* No parrallelised decompression for chunked second-stage content - chunked frames will still be decoded but offer no performance benefit. 
* Only supports AVIs containing a single video stream.

#Credits:
Thanks to:
* [Will Young](http://echoandreflection.co.uk)
* [Elliot Woods](https://github.com/elliotwoods)
* [Tebjan Halm](https://github.com/tevjan)
* [vux](https://github.com/mrvux)

#Open Source
Uses bits of:
* vux's [dx11-vvvv](https://github.com/mrvux/dx11-vvvv) nodes 
* [Snappy.NET](https://snappy.angeloflogic.com/)
* [VVVV](https://www.nuget.org/profiles/vvvv)

#License
BSD 3-Clause. See LICENSE.md
