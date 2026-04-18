using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using ii.BrightRespite.Model;
using System.Diagnostics.CodeAnalysis;

namespace ii.BrightRespite;

public class TilProcessor
{
	private const string ExpectedSignature = "TILE";

	private string filename { get; set; }
	private FileStream tilFileStream { get; set; }
	private BinaryReader tilBinaryReader { get; set; }
	public PalFile Palette { get; set; }

	public class TilFileData
	{
		public List<Image<Rgba32>> Tiles { get; set; } = [];
		public int TileSize { get; set; }
	}

	[Experimental("EXP001")]
	public void Open(string tilFilename)
	{
		tilFileStream = new FileStream(tilFilename, FileMode.Open, FileAccess.Read);
		tilBinaryReader = new BinaryReader(tilFileStream);
		filename = tilFilename;
	}

	public TilFileData Parse()
	{
		var result = new TilFileData();

		var signature = new string(tilBinaryReader.ReadChars(4));
		if (signature != ExpectedSignature)
		{
			throw new InvalidOperationException($"Unhandled TIL signature: {signature}");
		}

		var version = tilBinaryReader.ReadInt32();
		var tileSize = tilBinaryReader.ReadInt32();

		result.TileSize = tileSize;

		var tiles = new List<byte[]>();

		// Tile 1 variations
		tilBinaryReader.BaseStream.Seek(0x1210, SeekOrigin.Begin);
		tiles.Add(tilBinaryReader.ReadBytes(0x1452 - 0x1210)); // 578 bytes

		tilBinaryReader.BaseStream.Seek(0x1452, SeekOrigin.Begin);
		tiles.Add(tilBinaryReader.ReadBytes(0x1692 - 0x1452)); // 576 bytes

		tilBinaryReader.BaseStream.Seek(0x1692, SeekOrigin.Begin);
		tiles.Add(tilBinaryReader.ReadBytes(0x18d4 - 0x1692)); // 578 bytes

		tilBinaryReader.BaseStream.Seek(0x18d4, SeekOrigin.Begin);
		tiles.Add(tilBinaryReader.ReadBytes(0x1b14 - 0x18d4)); // 576 bytes

		tilBinaryReader.BaseStream.Seek(0x1b14, SeekOrigin.Begin);
		tiles.Add(tilBinaryReader.ReadBytes(0x1d56 - 0x1b14)); // 578 bytes

		tilBinaryReader.BaseStream.Seek(0x1d56, SeekOrigin.Begin);
		tiles.Add(tilBinaryReader.ReadBytes(0x1f9a - 0x1d56)); // 580 bytes

		tilBinaryReader.BaseStream.Seek(0x1f9a, SeekOrigin.Begin);
		tiles.Add(tilBinaryReader.ReadBytes(0x21d8 - 0x1f9a)); // 574 bytes

		tilBinaryReader.BaseStream.Seek(0x21d8, SeekOrigin.Begin);
		tiles.Add(tilBinaryReader.ReadBytes(0x241a - 0x21d8)); // 578 bytes

		// Tile 2 variations
		tilBinaryReader.BaseStream.Seek(0x241a, SeekOrigin.Begin);
		tiles.Add(tilBinaryReader.ReadBytes(0x265a - 0x241a)); // 576 bytes

		tilBinaryReader.BaseStream.Seek(0x265a, SeekOrigin.Begin);
		tiles.Add(tilBinaryReader.ReadBytes(0x289a - 0x265a)); // 576 bytes

		tilBinaryReader.BaseStream.Seek(0x289a, SeekOrigin.Begin);
		tiles.Add(tilBinaryReader.ReadBytes(0x2ada - 0x289a)); // 576 bytes

		tilBinaryReader.BaseStream.Seek(0x2ada, SeekOrigin.Begin);
		tiles.Add(tilBinaryReader.ReadBytes(0x2d1d - 0x2ada)); // 579 bytes

		tilBinaryReader.BaseStream.Seek(0x2d1d, SeekOrigin.Begin);
		tiles.Add(tilBinaryReader.ReadBytes(0x2f5f - 0x2d1d)); // 578 bytes

		tilBinaryReader.BaseStream.Seek(0x2f5f, SeekOrigin.Begin);
		tiles.Add(tilBinaryReader.ReadBytes(0x329f - 0x2f5f)); // 576 bytes

		tilBinaryReader.BaseStream.Seek(0x329f, SeekOrigin.Begin);
		tiles.Add(tilBinaryReader.ReadBytes(0x33df - 0x329f)); // 576 bytes

		int i = 0;
		foreach (var tile in tiles)
		{
			var tileImage = new Image<Rgba32>(24, 24);

			for (int y = 0; y < 24; y++)
			{
				for (int x = 0; x < 24; x++)
				{
					int pixelIndex = y * 24 + x;

					if (pixelIndex < tile.Length)
					{
						byte paletteIndex = tile[pixelIndex];

						if (paletteIndex < 256 && Palette?.PrimaryPalette != null && paletteIndex < Palette.PrimaryPalette.Count)
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

			string outputFilename = Path.Combine(Path.GetDirectoryName(filename) ?? ".", $"tile_{i:D4}.png");
			tileImage.Save(outputFilename);

			result.Tiles.Add(tileImage);

			i++;
		}


		return result;
	}
	public void Write(string filename)
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

	public void Close()
	{
		tilBinaryReader?.Close();
		tilFileStream?.Close();
	}
}