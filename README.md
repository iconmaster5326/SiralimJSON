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

Then install Aurie and AurieSharp (PREVIEW VERSIONS!) into `SiralimUltimateModded`.

### Running the Dumper

```bash
cp SiralimDumper/obj/x64/Debug/net9.0/SiralimDumper.dll SiralimUltimateModded/mods/Managed/SiralimDumper.dll
cd SiralimUltimateModded
./SiralimUltimate.exe
cd ..
```

### Running Fixups

```bash
./patchWithFixups.ps1
```

### Running the Image Mapper

```bash
UndertaleModCli load SiralimUltimateModded/data.win -s ImageMapper.csx
```

### Uploading to GitHub

```bash
./prepareExports.ps1
```
