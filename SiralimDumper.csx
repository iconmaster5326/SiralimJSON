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
            var outPath = $@"exported\images\{mapping.Output}";

            if (mapping.Frame >= sprite.Textures.Count)
            {
                Console.WriteLine($"FOUND BAD FRAME! {sprite.Name.Content}, frame {mapping.Frame}, to {outPath}");
                continue;
            }

            var page = sprite.Textures[mapping.Frame].Texture;
            lock (page.TexturePage.TextureData)
            {
                Console.WriteLine($"Writing {sprite.Name.Content}, frame {mapping.Frame}, to {outPath}");

                var cacheKey = $"{sprite.Name.Content} {mapping.Frame}";
                MagickImage inputImage;
                if (cache.ContainsKey(cacheKey))
                {
                    inputImage = cache[cacheKey];
                }
                else
                {
                    inputImage = worker.GetEmbeddedTexture(page.TexturePage);
                    cache[cacheKey] = inputImage;
                }

                MagickImage outputImage = new MagickImage(MagickColors.Transparent, sprite.Width, sprite.Height);
                outputImage.CopyPixels(inputImage, new MagickGeometry(page.SourceX, page.SourceY, page.SourceWidth, page.SourceHeight), page.TargetX, page.TargetY);

                Directory.CreateDirectory(Path.GetDirectoryName(outPath));
                outputImage.Write(outPath);
            }
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
