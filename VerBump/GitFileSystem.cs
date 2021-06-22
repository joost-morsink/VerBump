using LibGit2Sharp;
using System.Linq;
using System.Collections.Generic;
using System.IO;

namespace VerBump
{
    public class GitFileSystem : IFileSystem
    {
        private readonly Repository _repo;
        private readonly Commit _commit;
        private readonly Tree _tree;

        public GitFileSystem(Repository repo, Commit commit, Tree tree = null)
        {
            _repo = repo;
            _commit = commit;
            _tree = tree ?? commit?.Tree;
        }
        public string Combine(string path1, string path2)
            => Path.Combine(path1, path2);

        private byte[] ReadStream(Stream str)
        {
            using var ms = new MemoryStream();
            using (str)
                str.CopyTo(ms);
            return ms.ToArray();
        }
        public byte[] GetContentBytes(string path)
            => ReadStream(((Blob)_tree[path].Target).GetContentStream());

        public string GetContentString(string path)
            => ((Blob)_tree[path].Target).GetContentText();

        public IEnumerable<(string name, IFileSystem system)> GetDirectories()
            => _tree == null
                ? Enumerable.Empty<(string, IFileSystem)>()
                : _tree.Where(e => e.TargetType == TreeEntryTargetType.Tree).Select(t => (t.Name, (IFileSystem)new GitFileSystem(_repo, _commit, t.Target.Peel<Tree>())));

        public IEnumerable<string> GetFiles()
            => _tree == null
                ? Enumerable.Empty<string>()
                : _tree.Where(e => e.TargetType == TreeEntryTargetType.Blob).Select(t => t.Name);

        public string Hash(string path)
            => _tree == null ? "" : _tree[path].Target.Sha;

        public IFileSystem Navigate(string path)
            => _tree == null ? this : new GitFileSystem(_repo, _commit, _tree[path].Target.Peel<Tree>());
    }
}
