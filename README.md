# SiralimJSON

This is a tool that produces data lovingly scraped from the video game Siralim Ultimate in JSON format, for easy use by other tools and software.

## Building the Quicktype files

```bash
mkdir SiralimDumper/gen
quicktype --lang csharp --src-lang schema --out SiralimDumper/gen/SiralimUltimateDatabase.cs schema/combined.yaml --additional-schema 'schema/*.yaml' --framework SystemTextJson
```

## Building the Dumper

1. Open `SiralimDumper` in Visual Studio 2022.
2. Make a `deps` directory in it and place `AurieSharpInterop.dll` inside.
3. Build.

## Getting a Modded Siralim Ultimate instance

```bash
mkdir SiralimUltimateModded/
cp <your Siralim Ultimate directory> SiralimUltimateModded/ -r
rm SiralimUltimateModded/*steam*.dll
```

Then install Aurie and AurieSharp (PREVIEW VERSIONS!) into `SiralimUltimateModded`.

## Running the Dumper

```bash
cp SiralimDumper/obj/x64/Debug/net9.0/SiralimDumper.dll SiralimUltimateModded/mods/Managed/SiralimDumper.dll
cd SiralimUltimateModded
./SiralimUltimate.exe
cd ..
```

## Running the Image Mapper

```bash
UndertaleModCli load SiralimUltimateModded/data.win -s ImageMapper.csx
```
