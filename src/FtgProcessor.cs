using ii.BrightRespite.Model;

namespace ii.BrightRespite;

public class FtgProcessor
{
    public List<(string filename, byte[] bytes)> Read(string filename)
    {
        using var fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read);
        using var binaryReader = new BinaryReader(fileStream);

        var identifier = binaryReader.ReadBytes(4);
        var directoryOffset = BitConverter.ToInt32(binaryReader.ReadBytes(4), 0);
        var fileCount = BitConverter.ToInt32(binaryReader.ReadBytes(4), 0);

        var fileEntries = new List<FtgFileInfo>();
        binaryReader.BaseStream.Seek(directoryOffset, SeekOrigin.Begin);
        for (var i = 0; i < fileCount; i++)
        {
            var entryFilename = binaryReader.ReadChars(28);
            var offset = BitConverter.ToInt32(binaryReader.ReadBytes(4), 0);
            var size = BitConverter.ToInt32(binaryReader.ReadBytes(4), 0);
            var info = new FtgFileInfo()
            {
                Filename = new string(entryFilename),
                Offset = offset,
                Size = size
            };
            fileEntries.Add(info);
        }

        var result = new List<(string filename, byte[] bytes)>();
        foreach (var fileEntry in fileEntries)
        {
            binaryReader.BaseStream.Seek(fileEntry.Offset, SeekOrigin.Begin);
            var fileData = binaryReader.ReadBytes(fileEntry.Size);
            result.Add((fileEntry.Filename.Split('\0')[0], fileData));
        }
        return result;
    }
}