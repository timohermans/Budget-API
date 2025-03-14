namespace Budget.Application.Interfaces;

public interface IFileSystem
{
    bool FileExists(string path);
    Stream OpenRead(string path);
}

public class FileSystem : IFileSystem
{
    public bool FileExists(string path)
    {
        return File.Exists(path);
    }

    public Stream OpenRead(string path)
    {
        return File.OpenRead(path);
    }
}
