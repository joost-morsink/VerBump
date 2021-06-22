using System.Collections.Generic;
using Semver;

namespace VerBump
{
    internal static class Utils
    {
        public static KeyValuePair<K, V> ToKeyValuePair<K, V>(this (K, V) tuple)
            => new KeyValuePair<K, V>(tuple.Item1, tuple.Item2);
        private static Dictionary<string, IVersionFinder> _versionFinders = new Dictionary<string, IVersionFinder>
        {
            ["cs"] = new CSharp()
        };
        public static IVersionFinder GetVersionFinder(this string name)
            => _versionFinders.TryGetValue(name, out var res) ? res : null;
        public static Change? Compare(this SemVersion fromVer, SemVersion toVer)
        {
            if (fromVer > toVer)
                return null;
            if (fromVer.Major == toVer.Major)
                if (fromVer.Minor == toVer.Minor)
                    if (fromVer.Patch == toVer.Patch)
                        return Change.Same;
                    else
                        return Change.Patch;
                else
                    return Change.Minor;
            else
                return Change.Major;
        }
        public static SemVersion Bump(this SemVersion ver, Change kind)
            => kind switch
            {
                Change.Major => new SemVersion(ver.Major + 1),
                Change.Minor => new SemVersion(ver.Major, ver.Minor + 1),
                Change.Patch => new SemVersion(ver.Major, ver.Minor, ver.Patch + 1),
                _ => ver
            };
    }
}
