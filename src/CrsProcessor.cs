using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace ii.BrightRespite;

public class CrsProcessor
{
    public List<Image<Rgba32>> Read(string filepath, List<(int r, int g, int b)> palette)
    {
        if (!File.Exists(filepath))
        {
            throw new InvalidDataException("Invalid CRS file format.");
        }

        using var fs = new FileStream(filepath, FileMode.Open);
        using var br = new BinaryReader(fs);
        var fileType = br.ReadChars(4);
        if (fileType[0] != 'C' || fileType[1] != 'R' || fileType[2] != 'S' || fileType[3] != 'R')
        {
            throw new InvalidDataException("Invalid CRS file format.");
        }

        var results = new List<Image<Rgba32>>();
        var unknown = br.ReadInt32();
        var cursorCount = br.ReadInt32();
        for (var icon = 0; icon < cursorCount; icon++)
        {
            var data = br.ReadBytes(1024);

            var image = new Image<Rgba32>(32, 32);
            for (int pixelIndex = 0; pixelIndex < 1024; pixelIndex++)
            {
                int x = pixelIndex % 32;
                int y = pixelIndex / 32;

                var grayValue = data[pixelIndex];
                var col = palette[grayValue];
                image[x, y] = new Rgba32(col.r, col.g, col.b);
            }

            results.Add(image);
        }

        return results;
    }
}