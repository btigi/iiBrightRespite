using ii.BrightRespite.Model;

namespace ii.BrightRespite;

public class FtgProcessor
{
    public List<FtgFileInfo> Files = [];
    private FileStream ftgFileStream;
    private BinaryReader ftgBinaryReader;

    public void Open(string ftgFilename)
    {
        ftgFileStream = new FileStream(ftgFilename, FileMode.Open, FileAccess.Read);
        ftgBinaryReader = new BinaryReader(ftgFileStream);
    }

    public void Parse()
    {
        Files.Clear();

        var identifier = ftgBinaryReader.ReadBytes(4);
        var directoryOffset = BitConverter.ToInt32(ftgBinaryReader.ReadBytes(4), 0);
        var fileCount = BitConverter.ToInt32(ftgBinaryReader.ReadBytes(4), 0);

        ftgBinaryReader.BaseStream.Seek(directoryOffset, SeekOrigin.Begin);
        for (int i = 0; i < fileCount; i++)
        {
            var filename = ftgBinaryReader.ReadChars(28);
            var offset = BitConverter.ToInt32(ftgBinaryReader.ReadBytes(4), 0);
            var size = BitConverter.ToInt32(ftgBinaryReader.ReadBytes(4), 0);
            var info = new FtgFileInfo()
            {
                Filename = new string(filename),
                Offset = offset,
                Size = size
            };
            Files.Add(info);
        }
    }

    public void Extract(string outputFilename, FtgFileInfo fileInfo)
    {
        ftgBinaryReader.BaseStream.Seek(fileInfo.Offset, SeekOrigin.Begin);
        var bytes = ftgBinaryReader.ReadBytes(fileInfo.Size);
        using (FileStream outFile = new FileStream(outputFilename, FileMode.Create,FileAccess.Write))
        {
            outFile.Write(bytes, 0, bytes.Length);
        }
    }

    public void Close()
    {
        ftgBinaryReader.Close();
        ftgFileStream.Close();
    }
}