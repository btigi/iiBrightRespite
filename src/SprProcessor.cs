using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace ii.BrightRespite;

public class SprProcessor
{
    /*
Terminology:
    section -> an animation purpose, e.g. walking, firing
      rotation -> the direction the sprite is facing
        frame -> the individual image of a sprite

HEADER
4 char  RSPR / SSPR
4 version (always 0x10020000)
4 frame count
4 rotation count (first frame should always point East, orientation moves counter
clockwise)
4 width
4 height
4 frame count (total?)
4 section count

FRAME INDICES
frame count (in this section?) * 4 - frame ordering (-indexed)

SECTIONS
4 first frame number
4 last frame number
4 framerate
4 hotspot count

ANIMATION INFORMATION?

FRAME INFORMATION

FRAMES
29x 1E (fill x pixels with the default colour, i.e. fill 29 x 1E pixels with the default colour)
01x 0B (fill x pixels with the default colour, i.e. fill 0B x 1E pixels with the default colour)
1   number of explicit bytes
x   explit bytes
01x 11 (fill x pixels with the default colour, i.e. fill 01 x 11 pixels with the default colour)
01x 09 (fill x pixels with the default colour, i.e. fill 01 x 09 pixels with the default colour)
...
27x 1E

once the default colour is specified, any remaining unspecified pixels are filled with it

HOTSPOTS
hot spot information is stored after the frame data
4 length of hotspot data (0 = no hotspots)
  -- data below is option, dependent on above value
  foreach frame:
    1 hotspot 1 x coord
    1 hotspot 1 y coord
    1 unknown (always 0x01)
    1 hotspot 2 x coord
    1 hotspot 2 y coord
    1 unknown (always 0x01)
    1 hotspot 3 x coord
    1 hotspot 3 y coord
    ...


max 6 sections:
 Movement
 Firing Animation
 Second Firing Animation
 Special Action Sections
 Idle Animation Sections
 Standing Animation

for each section...

  .---> rotations
  |     walk_east   walk_north  walk_west   walk_south
  |     frame_e1    frame_n1    frame_w1    frame_s1
frames  frame_e2    frame_n2    frame_w2
        frame_e3                frame_w3
        frame_e4                frame_w4
        frame_e5                
*/

    private const int HeaderSize = 32;
    private const int FrameOrderSize = 4;
    private const int SectionSize = 16;
    private const string SignatureSprite = "RSPR";
    private const string SignatureShadow = "SSPR";
    private const int Version = 528;

    private const int DEFAULTCOLOUR = 0;

    private static readonly List<(int r, int g, int b)> DefaultPalette =
    [
        (0, 0, 0), (3, 3, 3), (5, 5, 5), (10, 10, 10), (15, 15, 15), (20, 20, 20), (25, 25, 25), (33, 33, 33),
        (40, 40, 40), (51, 51, 51), (61, 61, 61), (76, 76, 76), (89, 89, 89), (99, 99, 99), (107, 107, 107), (255, 255, 255),
        (0, 0, 0), (0, 0, 0), (0, 0, 0), (0, 0, 0), (0, 0, 0), (0, 0, 0), (255, 8, 0), (255, 153, 2),
        (255, 255, 2), (85, 250, 3), (2, 92, 255), (0, 0, 0), (2, 255, 255), (2, 75, 75), (119, 255, 135), (2, 69, 10),
        (44, 15, 72), (66, 17, 112), (98, 19, 170), (117, 18, 207), (138, 19, 246), (148, 53, 250), (153, 83, 255), (156, 111, 255),
        (77, 5, 0), (106, 5, 10), (127, 16, 16), (150, 21, 21), (176, 27, 27), (202, 35, 35), (235, 42, 66), (255, 61, 91),
        (72, 47, 13), (112, 59, 23), (150, 74, 11), (190, 96, 11), (229, 120, 10), (254, 145, 25), (255, 164, 64), (255, 187, 104),
        (43, 43, 11), (58, 66, 13), (65, 96, 15), (44, 113, 16), (22, 135, 16), (15, 154, 32), (21, 172, 100), (20, 187, 143),
        (3, 58, 49), (7, 78, 73), (4, 100, 101), (5, 119, 123), (6, 137, 146), (7, 156, 168), (16, 169, 211), (25, 184, 240),
        (5, 17, 82), (8, 30, 97), (34, 47, 129), (41, 59, 164), (48, 69, 197), (54, 83, 225), (76, 97, 253), (92, 115, 255),
        (29, 12, 10), (46, 21, 13), (63, 30, 17), (83, 41, 22), (101, 51, 26), (121, 60, 30), (143, 70, 44), (171, 90, 72),
        (93, 76, 72), (121, 100, 96), (131, 131, 131), (155, 155, 155), (179, 179, 179), (203, 203, 203), (200, 214, 233), (211, 228, 240),
        (23, 1, 1), (66, 1, 7), (118, 6, 11), (169, 6, 6), (208, 22, 7), (229, 38, 6), (241, 62, 6), (254, 84, 7),
        (254, 110, 21), (254, 134, 35), (255, 181, 60), (245, 208, 69), (255, 234, 99), (255, 255, 136), (11, 11, 12), (22, 21, 38),
        (32, 35, 64), (44, 45, 102), (78, 66, 161), (110, 92, 225), (2, 0, 2), (10, 2, 7), (24, 7, 25), (38, 11, 34),
        (47, 18, 49), (60, 27, 74), (68, 40, 81), (90, 59, 110), (141, 104, 154), (94, 95, 42), (18, 19, 1), (33, 30, 8),
        (39, 37, 16), (112, 109, 63), (65, 52, 0), (60, 63, 14), (85, 115, 28), (137, 121, 69), (168, 139, 43), (126, 116, 45),
        (171, 162, 85), (201, 207, 134), (93, 148, 62), (8, 22, 27), (21, 34, 38), (30, 48, 56), (51, 69, 79), (76, 101, 117),
        (46, 66, 46), (60, 87, 60), (102, 117, 104), (124, 127, 68), (19, 4, 6), (33, 12, 15), (20, 2, 0), (30, 6, 0),
        (56, 16, 5), (38, 22, 11), (76, 51, 37), (125, 84, 61), (161, 130, 111), (39, 161, 214), (126, 198, 238), (185, 255, 255),
        (0, 0, 0), (0, 0, 0), (0, 0, 0), (0, 0, 0), (0, 0, 0), (0, 0, 0), (0, 0, 0), (0, 0, 0), (0, 0, 0), (0, 0, 0),
        (0, 0, 0), (0, 0, 0), (0, 0, 0), (0, 0, 0), (0, 0, 0), (0, 0, 0), (0, 0, 0), (0, 0, 0), (0, 0, 0), (0, 0, 0),
        (0, 0, 0), (0, 0, 0), (0, 0, 0), (0, 0, 0), (0, 0, 0), (0, 0, 0), (0, 0, 0), (0, 0, 0), (0, 0, 0), (0, 0, 0),
        (0, 0, 0), (0, 0, 0), (0, 0, 0), (0, 0, 0), (0, 0, 0), (0, 0, 0), (0, 0, 0), (0, 0, 0), (0, 0, 0), (0, 0, 0),
        (0, 0, 0), (0, 0, 0), (0, 0, 0), (0, 0, 0), (0, 0, 0), (0, 0, 0), (0, 0, 0), (0, 0, 0), (0, 0, 0), (0, 0, 0),
        (0, 0, 0), (0, 0, 0), (0, 0, 0), (0, 0, 0), (0, 0, 0), (0, 0, 0), (0, 0, 0), (0, 0, 0), (0, 0, 0), (0, 0, 0),
        (0, 0, 0), (0, 0, 0), (0, 0, 0), (0, 0, 0), (0, 0, 0), (0, 0, 0), (0, 0, 0), (0, 0, 0), (0, 0, 0), (0, 0, 0),
        (0, 0, 0), (0, 0, 0), (0, 0, 0), (0, 0, 0), (0, 0, 0), (0, 0, 0), (0, 0, 0), (0, 0, 0), (0, 0, 0), (0, 0, 0),
        (0, 0, 0), (0, 0, 0), (0, 0, 0), (0, 0, 0), (0, 0, 0), (0, 0, 0), (0, 0, 0), (0, 0, 0), (0, 0, 0), (0, 0, 0),
        (0, 0, 0), (0, 0, 0), (0, 0, 0), (0, 0, 0), (0, 0, 0), (0, 0, 0), (0, 0, 0), (0, 0, 0), (0, 0, 0), (0, 0, 0),
        (0, 0, 0), (0, 0, 0), (0, 0, 0), (0, 0, 0), (0, 0, 0), (0, 0, 0), (0, 0, 0), (0, 0, 0), (0, 0, 0), (0, 0, 0),
        (0, 0, 0), (0, 0, 0), (0, 0, 0), (0, 0, 0), (0, 0, 0), (0, 0, 0)
    ];

    private FileStream sprFileStream { get; set; }
    private BinaryReader sprBinaryReader { get; set; }
    public List<(int r, int g, int b)> Palette { get; set; }

    public void Open(string sprFilename)
    {
        sprFileStream = new FileStream(sprFilename, FileMode.Open, FileAccess.Read);
        sprBinaryReader = new BinaryReader(sprFileStream);
    }

    public List<Image<Rgba32>> Parse()
    {
        var result = new List<Image<Rgba32>>();

        var identifier = new string(sprBinaryReader.ReadChars(4));
        if (identifier != SignatureSprite && identifier != SignatureShadow)
        {
            throw new InvalidOperationException($"Unhandled SPR type: {identifier}");
        }

        bool isShadow = identifier == SignatureShadow;


        var version = sprBinaryReader.ReadInt32();
        if (version != Version)
        {
            throw new InvalidOperationException($"Unhandled SPR version: {version}");
        }

        var framesPerSectionCount = sprBinaryReader.ReadInt32();
        var rotationCount = sprBinaryReader.ReadInt32();
        var width = sprBinaryReader.ReadInt32();
        var height = sprBinaryReader.ReadInt32();
        var frameCountTotal = sprBinaryReader.ReadInt32();
        var sectionCount = sprBinaryReader.ReadInt32();

        var sectionOffset = HeaderSize + (4 * framesPerSectionCount * rotationCount);
        var animationOffset = sectionOffset + (16 * sectionCount);
        var frameInfoOffset = animationOffset + (4 * framesPerSectionCount);
        var frameDataOffset = frameInfoOffset + (8 * frameCountTotal) + 4;

        var totalHotspots = 0;

        // An array of frame indices, e.g. if there are 3 frames this lists 0,1,2
        for (var i = 0; i < framesPerSectionCount * rotationCount; i++)
        {
            var frameOrder = sprBinaryReader.ReadInt32();
        }

        // Section information
        sprBinaryReader.BaseStream.Position = sectionOffset;
        for (var i = 0; i < sectionCount; i++)
        {
            var sectionFirst = sprBinaryReader.ReadInt32();
            var sectionLast = sprBinaryReader.ReadInt32();
            var sectionFrameRate = sprBinaryReader.ReadInt32();
            var sectionHotSpotCount = sprBinaryReader.ReadInt32();
            totalHotspots += sectionHotSpotCount;
        }

        // Animation data section (4 * framesPerSectionCount bytes) - section number for each animation
        // We read sections info above so we skip this data here

        // Frame information (frameCountTotal entries of 8 bytes each)
        sprBinaryReader.BaseStream.Position = frameInfoOffset;
        var frameInfos = new List<FrameInfo>();
        for (var i = 0; i < frameCountTotal; i++)
        {
            var dataOffset = sprBinaryReader.ReadInt32();
            var hotspotOffset = sprBinaryReader.ReadInt32();
            var frameInfo = new FrameInfo();
            frameInfo.DataPosition = dataOffset;
            frameInfo.HotspotOffset = hotspotOffset;
            frameInfos.Add(frameInfo);
        }

        /*
        The shadow sprite treats each pixel as on (coloured) or off (transparent)
        Read through the data bytes, with the first byte being how many off pixels to 
        draw, the second byte being how many on pixels to draw, the third being how
        many off pixels to draw and so on. The pattern resets whenever the number of        
        */

        // Frame data
        byte dataByte;
        for (var frame = 0; frame < frameCountTotal; frame++)
        {
            var data = new byte[width * height * 3];

            // Seek to frame data position (calculated from frame data offset + frame data position)
            sprBinaryReader.BaseStream.Seek(frameDataOffset + frameInfos[frame].DataPosition, SeekOrigin.Begin);

            // Process sprite data using RLE decompression
            var dataPosition = 0;
            var totalPixels = width * height;
            var maxDataIndex = totalPixels * 3;

            for (var currentRow = 0; currentRow < height; currentRow++)
            {
                var step = 0;
                var widthProgress = 0;

                while (widthProgress < width)
                {
                    // Bounds check for reading
                    if (sprBinaryReader.BaseStream.Position >= sprBinaryReader.BaseStream.Length)
                    {
                        throw new InvalidOperationException($"Unexpected end of file while reading frame {frame}");
                    }

                    dataByte = sprBinaryReader.ReadByte();
                    var cnt = dataByte;

                    // Mask high bit for colored pixels (step & 1)
                    if ((step & 1) != 0)
                    {
                        cnt &= 0x7f;
                    }

                    // Bounds check for pixel count
                    if (widthProgress + cnt > width)
                    {
                        throw new InvalidOperationException($"Pixel count exceeds row width: {widthProgress + cnt} > {width}");
                    }

                    if ((step & 1) != 0)
                    {
                        // Draw colored pixels
                        if (!isShadow)
                        {
                            // Regular sprite: read actual color data
                            for (var i = 0; i < cnt; i++)
                            {
                                if (dataPosition + 2 >= maxDataIndex)
                                {
                                    throw new InvalidOperationException($"Data buffer overflow at frame {frame}");
                                }

                                var colourByte = sprBinaryReader.ReadByte();
                                var colour = GetColour(colourByte);
                                data[dataPosition] = colour.B;
                                data[dataPosition + 1] = colour.G;
                                data[dataPosition + 2] = colour.R;
                                dataPosition += 3;
                            }
                        }
                        else
                        {
                            // Shadow sprite: draw white pixels
                            for (var i = 0; i < cnt; i++)
                            {
                                if (dataPosition + 2 >= maxDataIndex)
                                {
                                    throw new InvalidOperationException($"Data buffer overflow at frame {frame}");
                                }

                                data[dataPosition] = 255;     // B
                                data[dataPosition + 1] = 255; // G
                                data[dataPosition + 2] = 255; // R
                                dataPosition += 3;
                            }
                        }
                    }
                    else
                    {
                        // Draw transparent/default color pixels
                        for (var i = 0; i < cnt; i++)
                        {
                            if (dataPosition + 2 >= maxDataIndex)
                            {
                                throw new InvalidOperationException($"Data buffer overflow at frame {frame}");
                            }

                            data[dataPosition] = DEFAULTCOLOUR;     // B
                            data[dataPosition + 1] = DEFAULTCOLOUR; // G
                            data[dataPosition + 2] = DEFAULTCOLOUR; // R
                            dataPosition += 3;
                        }
                    }

                    widthProgress += cnt;
                    step++;
                }

                // Ensure we processed exactly the expected width
                if (widthProgress != width)
                {
                    throw new InvalidOperationException($"Row width mismatch: expected {width}, got {widthProgress}");
                }
            }

            var image = new Image<Rgba32>(width, height);
            image.ProcessPixelRows(accessor =>
            {
                var src = 0;
                for (var y = 0; y < height; y++)
                {
                    var pixelRow = accessor.GetRowSpan(y);
                    for (var x = 0; x < width; x++)
                    {
                        var b = data[src];
                        var g = data[src + 1];
                        var r = data[src + 2];
                        pixelRow[x] = new Rgba32(r, g, b, 255);
                        src += 3;
                    }
                }
            });
            result.Add(image);
        }

        if (totalHotspots > 0)
        {
            try
            {
                sprBinaryReader.BaseStream.Seek(frameInfoOffset + (8 * frameCountTotal), SeekOrigin.Begin);
                var hotspotRelativeOffset = sprBinaryReader.ReadInt32();
                var hotspotAbsoluteOffset = frameDataOffset + hotspotRelativeOffset;

                sprBinaryReader.BaseStream.Seek(hotspotAbsoluteOffset, SeekOrigin.Begin);
                var hotSpotCount = sprBinaryReader.ReadInt32();

                for (var i = 0; i < hotSpotCount; i++)
                {
                    var hotSpotX = sprBinaryReader.ReadByte();
                    var hotSpotY = sprBinaryReader.ReadByte();
                    var unknown = sprBinaryReader.ReadByte(); // should be 0x01
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Could not read hotspot data: {ex.Message}");
            }
        }

        return result;
    }

    private Rgba32 GetColour(byte paletteIndex)
    {
        Rgba32 color = default;
        var activePalette = Palette ?? DefaultPalette;
        
        if (paletteIndex < activePalette.Count)
        {
            var (r, g, b) = activePalette[paletteIndex];
            color = new Rgba32((byte)r, (byte)g, (byte)b, 255);
        }
        else if (paletteIndex == 0)
        {
            // Default/transparent color - return black
            color = new Rgba32(0, 0, 0, 255);
        }
        else
        {
            // Fallback - return a visible color to indicate missing palette entry
            color = new Rgba32(255, 0, 255, 255); // Magenta
        }
        return color;
    }

    public void Close()
    {
        sprBinaryReader?.Close();
        sprFileStream?.Close();
    }

    public class FrameInfo
    {
        public int DataPosition { get; set; }
        public int HotspotOffset { get; set; }
    }
}