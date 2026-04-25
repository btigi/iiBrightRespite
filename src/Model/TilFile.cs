using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace ii.BrightRespite.Model;

public class TilFile
{
	public List<Image<Rgba32>> LandTiles { get; set; } = [];
	public List<Image<Rgba32>> WaterTiles { get; set; } = [];
	public List<Image<Rgba32>> CoastTiles { get; set; } = [];
	public List<Image<Rgba32>> Masks { get; set; } = [];
	public int TileSize { get; set; }
}