using ii.BrightRespite.Model;

namespace ii.BrightRespite;

public class PalProcessor
{
    public PalFile Read(string filepath)
    {
        if (!File.Exists(filepath))
        {
            throw new InvalidDataException($"Invalid palette file.");
        }

        using var fs = new FileStream(filepath, FileMode.Open);
        using var br = new BinaryReader(fs);
        var fileType = br.ReadChars(4);
        if (fileType[0] != 'P' || fileType[1] != 'A' || fileType[2] != 'L' || fileType[3] != 'S')
        {
            throw new InvalidDataException("Invalid PAL file format.");
        }

        var version = br.ReadInt32();
        if (version != 258)
        {
            throw new InvalidDataException("Invalid PAL file version.");
        }

        var redEntries = br.ReadBytes(256);
        var greenEntries = br.ReadBytes(256);
        var blueEntries = br.ReadBytes(256);

        var result = new PalFile();
        result.PrimaryPalette = new List<(int r, int g, int b)>(256);
        for (int i = 0; i < 256; i++)
        {
            var r = redEntries[i]*4;
            var g = greenEntries[i] * 4;
            var b = blueEntries[i] * 4;
            result.PrimaryPalette.Add((r, g, b));
        }

        redEntries = br.ReadBytes(256);
        greenEntries = br.ReadBytes(256);
        blueEntries = br.ReadBytes(256);

        result.SecondaryPalette = new List<(int r, int g, int b)>(256);
        for (int i = 0; i < 768 / 3; i++)
        {
            var r = redEntries[i];
            var g = greenEntries[i];
            var b = blueEntries[i];
            result.SecondaryPalette.Add((r, g, b));
        }

        result.Unknown1 = br.ReadBytes(32768);
        result.Unknown2 = br.ReadBytes(147456 - (int)br.BaseStream.Position);
        result.ShadingTables = br.ReadBytes(147456);

        return result;
    }
}