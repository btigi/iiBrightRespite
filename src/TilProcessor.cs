using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using ii.BrightRespite.Model;

namespace ii.BrightRespite;

public partial class TilProcessor
{
    private const string ExpectedSignature = "TILE";

    private const long TileDataStartOffset = 0x1210;

    private const int TilePixelDimension = 24;
    private const int TilePixelCount = TilePixelDimension * TilePixelDimension;
    private const int LandTileCount = 120;
    private const int WaterTileCount = 64;
    private const int CoastTileCount = 64;
    private const int MaskTileCount = 259;

    public PalFile Palette { get; set; }

    public TilFile Read(string tilFilename)
    {
		using var fs = new FileStream(tilFilename, FileMode.Open, FileAccess.Read);
		using var br = new BinaryReader(fs);

		var result = new TilFile();

        var signature = new string(br.ReadChars(4));
        if (signature != ExpectedSignature)
        {
            throw new InvalidOperationException($"Unhandled TIL signature: {signature}");
        }

        var version = br.ReadInt32();
        var tileSize = br.ReadInt32();

        result.TileSize = tileSize;

        var stream = br.BaseStream;
        var bytesFromStartToEnd = stream.Length - TileDataStartOffset;
        if (bytesFromStartToEnd < 0)
        {
            throw new InvalidOperationException($"TIL file is too short: length {stream.Length} is before tile data offset {TileDataStartOffset}.");
        }

        stream.Seek(TileDataStartOffset, SeekOrigin.Begin);

        var landTiles = ReadTiles(br, LandTileCount, "land");
        var waterTiles = ReadTiles(br, WaterTileCount, "water");
        var coastTiles = ReadTiles(br, CoastTileCount, "coast");
        var maskTiles = ReadTiles(br, MaskTileCount, "mask");

        AddTileImages(landTiles, result.LandTiles);
        AddTileImages(waterTiles, result.WaterTiles);
        AddTileImages(coastTiles, result.CoastTiles);
        AddTileImages(maskTiles, result.Masks);

        return result;
    }

    private List<byte[]> ReadTiles(BinaryReader br, int tileCount, string groupName)
    {
        var tiles = new List<byte[]>(tileCount);
        for (var i = 0; i < tileCount; i++)
        {
            _ = br.ReadByte();
            var pixelData = br.ReadBytes(TilePixelCount);
            if (pixelData.Length != TilePixelCount)
            {
                throw new EndOfStreamException($"Expected {TilePixelCount} bytes of {groupName} tile data; got {pixelData.Length} at tile index {i}.");
            }

            tiles.Add(pixelData);
        }

        return tiles;
    }

    private void AddTileImages(IEnumerable<byte[]> tiles, List<Image<Rgba32>> target)
    {
        foreach (var tile in tiles)
        {
            var tileImage = CreateTileImage(tile);
            target.Add(tileImage);
        }
    }

    private Image<Rgba32> CreateTileImage(byte[] tile)
    {
        var tileImage = new Image<Rgba32>(TilePixelDimension, TilePixelDimension);

        for (int y = 0; y < TilePixelDimension; y++)
        {
            for (int x = 0; x < TilePixelDimension; x++)
            {
                int pixelIndex = y * TilePixelDimension + x;

                if (pixelIndex < tile.Length)
                {
                    byte paletteIndex = tile[pixelIndex];

                    if (Palette?.PrimaryPalette != null && paletteIndex < Palette.PrimaryPalette.Count)
                    {
                        var paletteColor = Palette.PrimaryPalette[paletteIndex];
                        var color = new Rgba32((byte)paletteColor.r, (byte)paletteColor.g, (byte)paletteColor.b, 255);
                        tileImage[x, y] = color;
                    }
                    else
                    {
                        tileImage[x, y] = new Rgba32(0, 0, 0, 255);
                    }
                }
                else
                {
                    tileImage[x, y] = new Rgba32(0, 0, 0, 0);
                }
            }
        }

        return tileImage;
    }

    public void Write(string filename, TilFile tilFile)
    {
        using var fileStream = new FileStream(filename, FileMode.Open, FileAccess.ReadWrite);
        using var writer = new BinaryWriter(fileStream);

        var tileOffsets = new[]
        {
            (0x1210, 576), // Tile 1 variation 1
            (0x1452, 576), // Tile 1 variation 2
            (0x1692, 576), // Tile 1 variation 3
            (0x18d4, 576), // Tile 1 variation 4
            (0x1b14, 576), // Tile 1 variation 5
            (0x1d56, 576), // Tile 1 variation 6
            (0x1f9a, 576), // Tile 1 variation 7
            (0x21d8, 576), // Tile 1 variation 8

            (0x241a, 576), // Tile 2 variation 1
            (0x265a, 576), // Tile 2 variation 2
            (0x289a, 576), // Tile 2 variation 3
            (0x2ada, 576), // Tile 2 variation 4
            (0x2d1d, 576), // Tile 2 variation 5
            (0x2f5f, 576), // Tile 2 variation 6
            (0x319f, 576), // Tile 2 variation 7
            (0x33DF, 576), // Tile 2 variation 8

            (0x361f, 576), // Tile 3 variation 1
            (0x3861, 576), // Tile 3 variation 2
            (0x3aa3, 576), // Tile 3 variation 3
            (0x3ce5, 576), // Tile 3 variation 4
            (0x3f25, 576), // Tile 3 variation 5
            (0x4167, 576), // Tile 3 variation 6
            (0x43a7, 576), // Tile 3 variation 7
            (0x45e9, 576), // Tile 3 variation 8

            (0x4829, 576), // Tile 4 variation 1
            (0x4a6b, 576), // Tile 4 variation 2
            (0x4cad, 576), // Tile 4 variation 3
            (0x4eeb, 576), // Tile 4 variation 4
            (0x512d, 576), // Tile 4 variation 5
            (0x536d, 576), // Tile 4 variation 6
            (0x55af, 576), // Tile 4 variation 7
            (0x57ef, 576), // Tile 4 variation 8

            (0x5a33, 576), // Tile 5 variation 1
            (0x5c71, 576), // Tile 5 variation 2
            (0x5eb3, 576), // Tile 5 variation 3
            (0x60f3, 576), // Tile 5 variation 4
            (0x6335, 576), // Tile 5 variation 5            
            (0x6575, 576), // Tile 5 variation 6
            (0x67b7, 576), // Tile 5 variation 7
            (0x69f7, 576), // Tile 5 variation 8

            (0x6c39, 576), // Tile 6 variation 1
            (0x6e7a, 576), // Tile 6 variation 2
            (0x70bb, 576), // Tile 6 variation 3
            (0x72fd, 576), // Tile 6 variation 4
            (0x753f, 576), // Tile 6 variation 5
            (0x777e, 576), // Tile 6 variation 6
            (0x79bf, 576), // Tile 6 variation 7
            (0x7c00, 576), // Tile 6 variation 8

            (0x7e42, 576), // Tile 7 variation 1
            (0x8082, 576), // Tile 7 variation 2
            (0x82c4, 576), // Tile 7 variation 3
            (0x8504, 576), // Tile 7 variation 4
            (0x8744, 576), // Tile 7 variation 5
            (0x8984, 576), // Tile 7 variation 6
            (0x8bc6, 576), // Tile 7 variation 7
            (0x8e08, 576), // Tile 7 variation 8

            (0x9049, 576), // Tile 8 variation 1
            (0x928a, 576), // Tile 8 variation 2
            (0x94cb, 576), // Tile 8 variation 3
            (0x970c, 576), // Tile 8 variation 4
            (0x994d, 576), // Tile 8 variation 5
            (0x9b8e, 576), // Tile 8 variation 6
            (0x9dc9, 576), // Tile 8 variation 7
            (0xa00a, 576), // Tile 8 variation 8
            /*
            (0xa24a, 576), // Tile 9 variation 1
            (0xa48c, 576), // Tile 9 variation 2
            (0xa6ce, 576), // Tile 9 variation 3
            (0xa910, 576), // Tile 9 variation 4
            (0xab52, 576), // Tile 9 variation 5
            (0xad94, 576), // Tile 9 variation 6
            (0xafd6, 576), // Tile 9 variation 7
            (0xb218, 576), // Tile 9 variation 8

            (0xb45a, 576), // Tile 10 variation 1
            (0xb69c, 576), // Tile 10 variation 2
            (0xb8de, 576), // Tile 10 variation 3
            (0xbb20, 576), // Tile 10 variation 4
            (0xbd62, 576), // Tile 10 variation 5
            (0xbfa4, 576), // Tile 10 variation 6
            (0xc1e6, 576), // Tile 10 variation 7
            (0xc428, 576), // Tile 10 variation 8

            (0xc66a, 576), // Tile 11 variation 1
            (0xc8ac, 576), // Tile 11 variation 2
            (0xcaee, 576), // Tile 11 variation 3
            (0xcd30, 576), // Tile 11 variation 4
            (0xcf72, 576), // Tile 11 variation 5
            (0xd1b4, 576), // Tile 11 variation 6
            (0xd3f6, 576), // Tile 11 variation 7
            (0xd638, 576), // Tile 11 variation 8

            (0xd87a, 576), // Tile 12 variation 1
            (0xdabc, 576), // Tile 12 variation 2
            (0xdcfe, 576), // Tile 12 variation 3
            (0xdf40, 576), // Tile 12 variation 4
            (0xe182, 576), // Tile 12 variation 5
            (0xe3c4, 576), // Tile 12 variation 6
            (0xe606, 576), // Tile 12 variation 7
            (0xe848, 576), // Tile 12 variation 8

            (0xea8a, 576), // Tile 13 variation 1
            (0xeccc, 576), // Tile 13 variation 2
            (0xef0e, 576), // Tile 13 variation 3
            (0xf150, 576), // Tile 13 variation 4
            (0xf392, 576), // Tile 13 variation 5
            (0xf5d4, 576), // Tile 13 variation 6
            (0xf816, 576), // Tile 13 variation 7
            (0xfa58, 576), // Tile 13 variation 8

            (0xfc9a, 576), // Tile 14 variation 1
            (0xfedc, 576), // Tile 14 variation 2
            (0x1011e, 576), // Tile 14 variation 3
            (0x10360, 576), // Tile 14 variation 4
            (0x105a2, 576), // Tile 14 variation 5
            (0x107e4, 576), // Tile 14 variation 6
            (0x10a26, 576), // Tile 14 variation 7
            (0x10c68, 576), // Tile 14 variation 8
            */
            (0x10eaa-41, 576), // Tile 15 variation 1
            (0x110ec, 576), // Tile 15 variation 2
            (0x1132e, 576), // Tile 15 variation 3
            (0x11570, 576), // Tile 15 variation 4
            (0x117b2, 576), // Tile 15 variation 5
            (0x119f4, 576), // Tile 15 variation 6
            (0x11c36, 576), // Tile 15 variation 7
            (0x11e78, 576), // Tile 15 variation 8
        };

        // Write pattern data to each tile location
        for (int i = 0; i < tileOffsets.Length; i++)
        {
            var (offset, size) = tileOffsets[i];

            var tileBytes = new byte[size];
            for (int j = 0; j < size; j++)
            {
                tileBytes[j] = (byte)(i + 1);
            }

            writer.BaseStream.Seek(offset, SeekOrigin.Begin);

            writer.Write(tileBytes);
        }
    }
}