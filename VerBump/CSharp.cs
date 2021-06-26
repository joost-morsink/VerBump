using System.Linq;
using System.Collections.Generic;
using System.Xml.Linq;
using Semver;

namespace VerBump
{
    public class CSharp : IVersionFinder
    {
        public IDictionary<string, SemVersion> GetVersions(IFileSystem fileSystem)
            => new Dictionary<string, SemVersion>(Search(fileSystem));
        public IEnumerable<string> Changed(IFileSystem old, IFileSystem @new)
        {
            foreach (var relPath in RelPaths("", @new))
            {
                var oldRel = old.Navigate(relPath.path);
                if (oldRel != null && IsChanged(oldRel, relPath.system))
                    yield return relPath.path;
            }
        }
        private bool IsChanged(IFileSystem old, IFileSystem @new)
        {
            var oldFiles = old.GetFiles().Where(n => !n.EndsWith(".csproj")).OrderBy(n => n).ToArray();
            var newFiles = @new.GetFiles().Where(n => !n.EndsWith(".csproj")).OrderBy(n => n).ToArray();
            var oldHashes = oldFiles.Select(old.Hash).ToArray();
            var newHashes = newFiles.Select(@new.Hash).ToArray();
            if (oldFiles.Length != newFiles.Length)
                return true;
            if (!oldFiles.Zip(newFiles, (o, n) => o == n && old.Hash(o) == @new.Hash(n)).All(x => x))
                return true;
            var oldDirs = old.GetDirectories().OrderBy(x => x.name).ToArray();
            var newDirs = @new.GetDirectories().OrderBy(x => x.name).ToArray();
            if (oldDirs.Length != newDirs.Length)
                return true;
            return oldDirs.Zip(newDirs, (o, n) => o.name != n.name || IsChanged(o.system, n.system)).Any(x => x);
        }

        private IEnumerable<(string path, IFileSystem system)> RelPaths(string relPath, IFileSystem fileSystem)
        {
            var csproj = fileSystem.GetFiles().Where(n => n.EndsWith(".csproj"));
            foreach (var cp in csproj.Take(1))
            {
                yield return (relPath, fileSystem);
            }
            foreach (var dir in fileSystem.GetDirectories())
                foreach (var tup in RelPaths(fileSystem.Combine(relPath, dir.name), dir.system))
                    yield return tup;
        }
        private IEnumerable<KeyValuePair<string, SemVersion>> Search(IFileSystem fileSystem)
            => from x in RelPaths("", fileSystem)
               from csproj in x.system.GetFiles().Where(n => n.EndsWith(".csproj")).Take(1)
               let e = XElement.Parse(x.system.GetContentString(csproj))
               from v in e.Descendants("VersionPrefix")
               select (x.path, SemVersion.TryParse(v.Value, out var res) ? res : null) into ver
               where ver.Item2 != null
               select ver.ToKeyValuePair();

        public void SetVersion(IWriteableFileSystem fileSystem, SemVersion version)
        {
            foreach (var f in fileSystem.GetFiles().Where(n => n.EndsWith(".csproj")).Take(1))
            {
                var e = XElement.Parse(fileSystem.GetContentString(f));
                foreach (var v in e.Descendants("VersionPrefix"))
                    v.Value = version.ToString();
                fileSystem.SetContent(f, e.ToString());
            }
        }
    }
}
