namespace ii.BrightRespite.Model;

public class PalFile
{
    public List<(int r, int g, int b)> PrimaryPalette { get; set; }
    public List<(int r, int g, int b)> SecondayrPalette { get; set; }
    public byte[] Unknown1 { get; set; }
    public byte[] Unknown2 { get; set; }
    public byte[] ShadingTables { get; set; }
}