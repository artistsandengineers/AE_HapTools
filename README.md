#Wat
Tools for working AVI files containing [Hap](https://github.com/Vidvox/hap) video, specifically:
* vvvv-HapTreats - a vvvv node for loading Textures from AVIs.
* hap2dds - CLI tool for extracting Hap encoded frames from AVIs and writing them to .dds files.
* AE_HapAVI - a class library on which the above are based.

#Building
1. `git clone git@bitbucket.org:arronsmith/vvvv-hap.git`
2. `cd vvvv-hap`
3. `git submodule update --init --recursive`
4. For reasons unknown Nuget fails to restore the Snappy packages when AE_HapAVI is built via a VS project reference. Open AE_HapAVI/AE_HapAVI.sln and build it before attempting to build other things. You might need to `Install-Package Snappy.NET` in the Nuget Package Manager Console.

#Limitations
* No support for AVI audio.
* Only supports DXT1/BC1 frame encoding (no Hap Alpha, Hap Q, Hap Q Alpha, Hap Alpha-Only), though this will be fixed soonish.
* Only supports AVIs containing a single video stream.

#Open Source
Uses bits of:
* vux's [dx11-vvvv](https://github.com/mrvux/dx11-vvvv) nodes 
* [Snappy.NET](https://snappy.angeloflogic.com/)
* [VVVV](https://www.nuget.org/profiles/vvvv)

#License
BSD 3-Clause. See LICENSE.md