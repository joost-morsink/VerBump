using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System;

namespace VerBump
{
    public class FileSystem : IWriteableFileSystem
    {
        private readonly string _basePath;

        public FileSystem(string basePath)
        {
            _basePath = basePath;
        }

        public string Combine(string path1, string path2)
            => Path.Combine(path1, path2);

        public byte[] GetContentBytes(string path)
            => File.ReadAllBytes(Combine(_basePath, path));

        public string GetContentString(string path)
            => File.ReadAllText(Combine(_basePath, path));

        IEnumerable<(string name, IFileSystem system)> IFileSystem.GetDirectories()
            => GetDirectories().Select(x => (x.name, (IFileSystem)x.system));

        public IEnumerable<string> GetFiles()
            => Directory.EnumerateFiles(_basePath).Select(Path.GetFileName);

        public string Hash(string path)
        {
            var content = GetContentBytes(path);
            var header = Encoding.UTF8.GetBytes($"blob #{content.Length}\u0000");
            
            return string.Concat(
                SHA1.Create()
                    .ComputeHash(new[] { header, content }.SelectMany(b => b)
                                                          .ToArray())
                    .Select(b => b.ToString("x2")));
        }
        IWriteableFileSystem IWriteableFileSystem.Navigate(string path)
            => Navigate(path);

        public FileSystem Navigate(string path)
        {
            var absPath = Path.Combine(_basePath, path);
            return Directory.Exists(absPath) ? new FileSystem(absPath) : null;
        }

        IFileSystem IFileSystem.Navigate(string path)
            => Navigate(path);

        public void SetContent(string path, string content)
        {
            File.WriteAllText(Path.Combine(_basePath, path), content);
        }

        public void SetContent(string path, byte[] content)
        {
            File.WriteAllBytes(Path.Combine(_basePath, path), content);
        }

        public IEnumerable<(string name, IWriteableFileSystem system)> GetDirectories()
            => Directory.EnumerateDirectories(_basePath).Select(d => (d, (IWriteableFileSystem)new FileSystem(d)));

    }
}
