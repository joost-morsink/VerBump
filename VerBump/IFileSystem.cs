using System.Collections.Generic;
namespace VerBump
{
    public interface IFileSystem
    {
        IEnumerable<(string name, IFileSystem system)> GetDirectories();
        IEnumerable<string> GetFiles();
        string GetContentString(string path);
        byte[] GetContentBytes(string path);
        string Combine(string path1, string path2);
        string Hash(string path);
        IFileSystem Navigate(string path);
    }
    public interface IWriteableFileSystem : IFileSystem
    {
        void SetContent(string path, string content);
        void SetContent(string path, byte[] content);
        new IEnumerable<(string name, IWriteableFileSystem system)> GetDirectories();
        new IWriteableFileSystem Navigate(string path);
    }
}
