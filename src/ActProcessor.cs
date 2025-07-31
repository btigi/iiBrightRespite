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
}