using LibGit2Sharp;
using System.Linq;
using System.Collections.Generic;
using System.IO;

namespace VerBump
{
    public class GitWorkTreeFileSystem : IWriteableFileSystem
    {
        private readonly string _basePath;
        private readonly Repository _repo;
        private readonly RepositoryStatus _status;
        private readonly GitFileSystem _git;
        private readonly FileSystem _worktree;
        private readonly string _path;
        public GitWorkTreeFileSystem(string basePath)
        {
            _basePath = basePath;
            _repo = new Repository(_basePath);
            _status = _repo.RetrieveStatus();
            _worktree = new FileSystem(_basePath);
            _git = new GitFileSystem(_repo, _repo.Head.Tip);
            _path = "";
        }
        private GitWorkTreeFileSystem(string basePath, Repository repo, RepositoryStatus status, GitFileSystem git, FileSystem worktree, string path)
        {
            _basePath = basePath;
            _repo = repo;
            _status = status;
            _git = git;
            _worktree = worktree;
            _path = path;
        }

        public string Combine(string path1, string path2)
            => Path.Combine(path1, path2);

        public byte[] GetContentBytes(string path)
            => _status[Combine(_path, path)].State == FileStatus.Unaltered
                ? _git.GetContentBytes(path)
                : _worktree.GetContentBytes(path);

        public string GetContentString(string path)
            => _status[Combine(_path, path)].State == FileStatus.Unaltered
                ? _git.GetContentString(path)
                : _worktree.GetContentString(path);

        public IEnumerable<(string name, GitWorkTreeFileSystem system)> GetDirectories()
            => _git.GetDirectories().Select(t => (t.name, new GitWorkTreeFileSystem(_basePath, _repo, _status, t.system, _worktree.Navigate(t.name), Combine(_path, t.name))));

        IEnumerable<(string name, IFileSystem system)> IFileSystem.GetDirectories()
            => GetDirectories().Select(d => (d.name, (IFileSystem)d.system));

        public IEnumerable<string> GetFiles()
            => _git.GetFiles().Where(fn => !_status.Removed.Any(se => se.FilePath == Combine(Combine(_basePath, _path), fn)))
                .Concat(_status.Added.Where(se => Path.GetDirectoryName(se.FilePath) == _path).Select(se => Path.GetFileName(se.FilePath)));

        public string Hash(string path)
            => _status[Combine(_path, path)].State == FileStatus.Nonexistent
                    || _status[Combine(_path, path)].State == FileStatus.Unaltered
                ? _git.Hash(path)
                : _worktree.Hash(path);

        public GitWorkTreeFileSystem Navigate(string path)
            => new GitWorkTreeFileSystem(_basePath, _repo, _status, _git.Navigate(path), _worktree.Navigate(path), Combine(_path, path));

        IFileSystem IFileSystem.Navigate(string path)
            => Navigate(path);

        public void SetContent(string path, string content)
            => _worktree.SetContent(path, content);

        public void SetContent(string path, byte[] content)
            => _worktree.SetContent(path, content);

        IEnumerable<(string name, IWriteableFileSystem system)> IWriteableFileSystem.GetDirectories()
            => GetDirectories().Select(d => (d.name, (IWriteableFileSystem)d.system));

        IWriteableFileSystem IWriteableFileSystem.Navigate(string path)
            => Navigate(path);
    }
}
