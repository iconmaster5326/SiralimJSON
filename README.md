# SiralimJSON

This is a tool that produces data lovingly scraped from the video game Siralim Ultimate in JSON format, for easy use by other tools and software.

## Accessing the Data

Data is available in three formats:

- `combined`: All the JSON is in one file, `combined.json`.
- `aggregate`: Each type of Siralim entity (creatures, spells, etc.) is in a single JSON file.
- `individual`: Each Siralim entity is in its own file. An index of what's in what file can be found in `individual.json`.

There are two ways of getting the data:

- To get a ZIP of the latest JSON data, click on the the "Releases" tab, to the right, and download the latest release! You can also use the download URLs to automatically download ZIPs.
- To grab particular JSON files you want to access directly, via `curl` or what have you, the latest data JSON is available under the `combined`, `aggregate`, and `individual` branches. To get a permalink to a JSON file, click on `Raw` and copy the URL.

## Using the JSON Schema

There are JSON Schema files under [schema](schema) for you to use for your own projects.

## Contributing Manual Fixups to SiralimJSON

Some data cannot be accurately scraped, and this data needs manually fixed up. If you'd like to fix data, or add useful notes to anything, you can make a merge request with changes to our manual fixups. See the [fixups](fixups) directory for more information.

## Contributing Code to SiralimJSON

### Building the Quicktype files

```bash
mkdir SiralimDumper/gen
quicktype --lang csharp --src-lang schema --out SiralimDumper/gen/SiralimUltimateDatabase.cs schema/combined.yaml --additional-schema 'schema/*.yaml' --framework SystemTextJson
```

### Building the Dumper

1. Open `SiralimDumper` in Visual Studio 2022.
2. Make a `deps` directory in it and place `AurieSharpInterop.dll` inside.
3. Build.

### Getting a Modded Siralim Ultimate instance

```bash
mkdir SiralimUltimateModded/
cp <your Siralim Ultimate directory> SiralimUltimateModded/ -r
rm SiralimUltimateModded/*steam*.dll
```

Then install Aurie and AurieSharp (PREVIEW VERSIONS!) into `SiralimUltimateModded`. The layout should look like:

```
SiralimUltimateModded/
    ...
    mods/
        Aurie/
            AurieSharpCore.dll
            AurieSharpInterop.dll
            AurieSharpInterop.runtimeconfig.json
            YYToolkit.dll
        Managed/
            AurieSharpManaged.dll
        Native/
            AurieCore.dll
            Ijwhost.dll
            nethost.dll
```

A command to use the patcher:

```bash
AuriePatcher.exe SiralimUltimate.exe mods/Native/AurieCore.dll install
```

### Running the Dumper

```bash
cp SiralimDumper/obj/x64/Debug/net9.0/SiralimDumper.dll SiralimUltimateModded/mods/Managed/SiralimDumper.dll
cd SiralimUltimateModded
./SiralimUltimate.exe
cd ..
```

The raw dumped data will be under `dumped`.

### Generating the Full Database

```bash
./patchWithFixups.ps1
./dumpedToExported.ps1
UndertaleModCli load SiralimUltimateModded/data.win -s ImageMapper.csx
```

The database will be under `exported`.

### Uploading to GitHub

```bash
./prepareExports.ps1
```

## TODO

### Scrapable Data

* Shop information - what can be bought, and for how much, at...
    * God shops (and at what ranks)
    * Ned (and at what ranks)
    * Arena shop (and at what ranks)
    * Guild shops (and at what ranks)
    * Dwarf shop
    * Everett
* Nether Boss information
    * creature correspondance
    * skin correspondance
    * refight dialogue
* Spell property long descriptions (?)
* Specialization decoration unlocked when ascending
* False God spell/decoration drops
* Depth some Realms are auto-unlocked at
* Which projects are accessible through normal gameplay - create new sandbox save and see what projects are either completed or can be immediately completed

### Data that needs Manual Fixups

* Creator information for all backer content
