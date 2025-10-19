namespace ii.BrightRespite;

public class ActProcessor
{
    public List<(int r, int g, int b)> Read(string filepath)
    {
        if (!File.Exists(filepath))
        {
            return [];
        }

        var bytes = File.ReadAllBytes(filepath);
        if (bytes.Length != 768)
        {
            throw new InvalidDataException($"Invalid ACT file size: {bytes.Length}. Expected 768 bytes.");
        }

        var results = new List<(int r, int g, int b)>(256);
        for (int i = 0; i < bytes.Length / 3; i++)
        {
            results.Add((
                r: bytes[i * 3],
                g: bytes[i * 3 + 1],
                b: bytes[i * 3 + 2]
            ));
        }
        return results;
    }

    public void Write(List<(int r, int g, int b)> colors, string filepath)
    {
        if (colors == null)
        {
            throw new ArgumentNullException(nameof(colors));
        }

        if (colors.Count != 256)
        {
            throw new ArgumentException($"Invalid color count: {colors.Count}. Expected 256 colors.");
        }

        var bytes = new byte[768]; // 256 colors * 3 bytes per color
        for (int i = 0; i < colors.Count; i++)
        {
            var (r, g, b) = colors[i];
            
            // Validate RGB values are in valid range
            if (r < 0 || r > 255 || g < 0 || g > 255 || b < 0 || b > 255)
            {
                throw new ArgumentException($"Invalid RGB values at index {i}: ({r}, {g}, {b}). Values must be between 0 and 255.");
            }

            bytes[i * 3] = (byte)r;
            bytes[i * 3 + 1] = (byte)g;
            bytes[i * 3 + 2] = (byte)b;
        }

        File.WriteAllBytes(filepath, bytes);
    }
}