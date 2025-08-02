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

        var bytes = File.ReadAllBytes(filepath);
        if (bytes.Length != 247560)
        {
            throw new InvalidDataException($"Invalid palette file size: {bytes.Length}. Expected 768 bytes.");
        }

        using var fs = new FileStream(filepath, FileMode.Open);
        using var br = new BinaryReader(fs);
        var fileType = br.ReadChars(4);
        if (fileType[0] != 'P' || fileType[1] != 'A' || fileType[2] != 'L' || fileType[3] != 'S')
        {
            throw new InvalidDataException("Invalid PAL file format.");
        }

        var version = br.ReadInt16();
        if (version != 258)
        {
            throw new InvalidDataException("Invalid PAL file version.");
        }


        var result = new PalFile();
        result.PrimaryPalette = new List<(int r, int g, int b)>(256);
        for (int i = 0; i < 768 / 3; i++)
        {
            result.PrimaryPalette.Add((
                r: bytes[i * 3],
                g: bytes[i * 3 + 1],
                b: bytes[i * 3 + 2]
            ));
        }

        result.SecondayrPalette = new List<(int r, int g, int b)>(256);
        for (int i = 0; i < 768 / 3; i++)
        {
            result.SecondayrPalette.Add((
                r: bytes[i * 3],
                g: bytes[i * 3 + 1],
                b: bytes[i * 3 + 2]
            ));
        }

        result.Unknown1 = br.ReadBytes(32768);
        result.Unknown2 = br.ReadBytes(147456 - (int)br.BaseStream.Position);
        result.ShadingTables = br.ReadBytes(147456);

        return result;
    }
}