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

    public void Write(string filename, List<(string filename, byte[] bytes)> files)
    {
        ArgumentNullException.ThrowIfNull(files);

        using var fileStream = new FileStream(filename, FileMode.Create, FileAccess.Write);
        using var binaryWriter = new BinaryWriter(fileStream);

        // Calculate directory offset (header + all file data)
        var headerSize = 12; // 4 bytes identifier + 4 bytes directory offset + 4 bytes file count
        var totalFileDataSize = files.Sum(f => f.bytes.Length);
        var directoryOffset = headerSize + totalFileDataSize;

        binaryWriter.Write(new byte[] { 0x46, 0x54, 0x47, 0x00 }); // "BOTG" identifier
        binaryWriter.Write(BitConverter.GetBytes(directoryOffset));
        binaryWriter.Write(BitConverter.GetBytes(files.Count));

        var fileInfos = new List<FtgFileInfo>();
        var currentOffset = headerSize;

        foreach (var (entryFilename, fileData) in files)
        {
            binaryWriter.Write(fileData);
            fileInfos.Add(new FtgFileInfo
            {
                Filename = entryFilename,
                Offset = currentOffset,
                Size = fileData.Length
            });
            
            currentOffset += fileData.Length;
        }

        foreach (var fileInfo in fileInfos)
        {
            var filenameBytes = new byte[28];
            var sourceBytes = System.Text.Encoding.ASCII.GetBytes(fileInfo.Filename);
            Array.Copy(sourceBytes, filenameBytes, Math.Min(sourceBytes.Length, 27)); // Leave at least one null terminator
            binaryWriter.Write(filenameBytes);
            binaryWriter.Write(BitConverter.GetBytes(fileInfo.Offset));
            binaryWriter.Write(BitConverter.GetBytes(fileInfo.Size));
        }
    }
}