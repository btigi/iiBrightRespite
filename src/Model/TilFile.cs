using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace ii.BrightRespite.Model;

public class TilFile
{
	public List<(byte LeadingByte, Image<Rgba32> Image)> LandTiles { get; set; } = [];
	public List<(byte LeadingByte, Image<Rgba32> Image)> CoastTiles { get; set; } = [];
	public List<Image<Rgba32>> WaterTiles { get; set; } = [];
	public List<(int? GroupHeader, Image<Rgba32> Image)> Masks { get; set; } = [];
	public List<Image<Rgba32>> EditorMasks { get; set; } = [];
	public int UnknownA { get; set; }
	public int UnknownB { get; set; }
}