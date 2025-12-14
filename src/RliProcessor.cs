using ii.BrightRespite.Model;
using System.Text;

namespace ii.BrightRespite;

public class RliProcessor
{
    /*
   
    .RLI (Resource List Index) - Directory file
    - 12 bytes: Header/signature
      - 4  bytes: ILR. : Signature
      - 4 bytes: Filesize
      - 4 bytes: File count?
    - Multiple 32-byte entries:
      - 12 bytes: File name (null-padded string)
      - 4 bytes:  File type (null-padded string)
      - 4 bytes:  Unknown
      - 4 bytes:  Offset in .rld file (little-endian)
      - 4 bytes:  File size in bytes (little-endian)
      - 4 bytes:  Unknown
    
    .RLD (Resource List Data) - Actual file data
    - Raw concatenated file data at offsets specified in .rli
    */

    public List<(string filename, byte[] bytes)> Read(string indexFilePath, string dataFilePath)
    {
        using var indexStream = new FileStream(indexFilePath, FileMode.Open, FileAccess.Read);
        using var dataStream = new FileStream(dataFilePath, FileMode.Open, FileAccess.Read);
        using var indexReader = new BinaryReader(indexStream);
        using var dataReader = new BinaryReader(dataStream);

        // Skip file header
        indexStream.Seek(12, SeekOrigin.Begin);

        var result = new List<(string filename, byte[] bytes)>();

        while (indexStream.Position < indexStream.Length)
        {
            // Directory entry
            var entryBytes = indexReader.ReadBytes(32);
            if (entryBytes.Length < 32)
                break;

            var entry = ParseDirectoryEntry(entryBytes);
            if (entry == null)
                continue;

            // File data
            dataReader.BaseStream.Seek(entry.Offset, SeekOrigin.Begin);
            var fileData = dataReader.ReadBytes((int)entry.Size);

            if (fileData.Length == entry.Size)
            {
                result.Add(($"{entry.Name}.tlf", fileData));
            }
        }

        return result;
    }

    public List<RliFileEntry> ListArchiveContents(string indexFilePath)
    {
        var entries = new List<RliFileEntry>();

        using var indexStream = new FileStream(indexFilePath, FileMode.Open, FileAccess.Read);
        using var indexReader = new BinaryReader(indexStream);

        // Skip file header
        indexStream.Seek(12, SeekOrigin.Begin);

        while (indexStream.Position < indexStream.Length)
        {
            // Directory entry
            var entryBytes = indexReader.ReadBytes(32);
            if (entryBytes.Length < 32)
                break;

            var entry = ParseDirectoryEntry(entryBytes);
            if (entry != null)
            {
                entries.Add(entry);
            }
        }

        return entries;
    }

    private RliFileEntry? ParseDirectoryEntry(byte[] entryBytes)
    {
        if (entryBytes.Length != 32)
            return null;

        var entry = new RliFileEntry();

        // File name (12 bytes, null-padded string)
        entry.Name = Encoding.ASCII.GetString(entryBytes, 0, 12).TrimEnd('\0');

        // File type (4 bytes, null-padded string)
        entry.Type = Encoding.ASCII.GetString(entryBytes, 12, 4).TrimEnd('\0');

        // Numeric fields (little-endian)
        entry.Unknown1 = BitConverter.ToUInt32(entryBytes, 16);
        entry.Offset = BitConverter.ToUInt32(entryBytes, 20);
        entry.Size = BitConverter.ToUInt32(entryBytes, 24);
        entry.Unknown2 = BitConverter.ToUInt32(entryBytes, 28);

        // Basic validation
        if (string.IsNullOrEmpty(entry.Name) || entry.Size == 0)
            return null;

        return entry;
    }
}