using System.Collections.Generic;
using Semver;

namespace VerBump
{
    public interface IVersionFinder
    {
        IDictionary<string, SemVersion> GetVersions(IFileSystem fileSystem);
        void SetVersion(IWriteableFileSystem fileSystem, SemVersion version);
        IEnumerable<string> Changed(IFileSystem old, IFileSystem @new);
    }
}
