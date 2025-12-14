namespace ii.BrightRespite.Model;


public class RliFileEntry
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public uint Unknown1 { get; set; }
    public uint Offset { get; set; }
    public uint Size { get; set; }
    public uint Unknown2 { get; set; }
}