using System;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Linq;
using System.Text.Json;

using ImageMagick;
using UndertaleModLib;
using UndertaleModLib.Scripting;
using UndertaleModLib.Models;
using UndertaleModLib.Util;

EnsureDataLoaded();

var mappings = JsonSerializer.Deserialize<Dictionary<string, List<ImageInfo>>>(File.ReadAllText(@"SiralimDumperImageMappings.json"));
var cache = new Dictionary<string, MagickImage>();
var worker = new TextureWorker();

foreach (var sprite in Data.Sprites)
{
    if (mappings.ContainsKey(sprite.Name.Content))
    {
        foreach (ImageInfo mapping in mappings[sprite.Name.Content])
        {
            if (mapping.Frame >= sprite.Textures.Count)
            {
                Console.WriteLine($"FOUND BAD FRAME! {sprite.Name.Content}, frame {mapping.Frame}, for {mapping.Output}");
                continue;
            }

            Dumper.Dump(mapping, sprite.Textures[mapping.Frame].Texture, sprite.Name.Content, sprite.Width, sprite.Height);
        }
    }
}

foreach (var tileset in Data.Backgrounds)
{
    if (mappings.ContainsKey(tileset.Name.Content))
    {
        foreach (ImageInfo mapping in mappings[tileset.Name.Content])
        {
            Dumper.Dump(mapping, tileset.Texture, tileset.Name.Content, tileset.Texture.TargetWidth, tileset.Texture.TargetHeight);
        }
    }
}

public static class Dumper
{
    public static Dictionary<string, MagickImage> Cache = new();
    public static TextureWorker Worker = new();
    public static void Dump(ImageInfo mapping, UndertaleTexturePageItem page, string name, uint w, uint h)
    {
        string outPath = $@"exported\images\{mapping.Output}";

        lock (page.TexturePage.TextureData)
        {
            Console.WriteLine($"Writing {name}, frame {mapping.Frame}, to {outPath}");

            var cacheKey = $"{name} {mapping.Frame}";
            MagickImage inputImage;
            if (Cache.ContainsKey(cacheKey))
            {
                inputImage = Cache[cacheKey];
            }
            else
            {
                inputImage = Worker.GetEmbeddedTexture(page.TexturePage);
                Cache[cacheKey] = inputImage;
            }

            MagickImage outputImage = new MagickImage(MagickColors.Transparent, w, h);
            outputImage.CopyPixels(inputImage, new MagickGeometry(page.SourceX, page.SourceY, page.SourceWidth, page.SourceHeight), page.TargetX, page.TargetY);

            Directory.CreateDirectory(Path.GetDirectoryName(outPath));
            outputImage.Write(outPath);
        }
    }
}

public class ImageInfo
{
    public int Frame { get; set; }
    public string Output { get; set; }

    public ImageInfo(int frame, string output)
    {
        Frame = frame;
        Output = output;
    }
}
