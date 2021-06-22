using System;
using LibGit2Sharp;
using System.Linq;
using System.Collections.Generic;
using CommandLine;
using System.Threading.Tasks;

namespace VerBump
{
    [Verb("info")]
    public class InfoCommand : BaseCommand
    {
        protected override Task Execute()
        {
            foreach (var (name, finder) in Config.Types.Select(t => (t, t.GetVersionFinder())))
            {

                Console.WriteLine($"Finder {name}:");
                var baseCommit = Config.Base == null ? null : Repo.Lookup<Commit>(Config.Base);
                var baseFs = new GitFileSystem(Repo, baseCommit);
                var baseVers = finder.GetVersions(baseFs);
                var fs = new GitFileSystem(Repo, Repo.Head.Tip);
                var changes = new HashSet<string>(finder.Changed(baseFs, fs));
                foreach (var ver in finder.GetVersions(fs).OrderBy(x => x.Key))
                {
                    if (baseVers.TryGetValue(ver.Key, out var baseVer))
                    {
                        if (ver.Value != baseVer)
                            Console.WriteLine($"{ver.Key}: {baseVer} => {ver.Value} ({baseVer.Compare(ver.Value)})");
                        else if (changes.Contains(ver.Key))
                            Console.WriteLine($"{ver.Key}: {ver.Value} !! CHANGED !!");
                        else
                            Console.WriteLine($"{ver.Key}: {ver.Value}");
                    }
                    else
                        Console.WriteLine($"{ver.Key}: {ver.Value} !! NEW !!");
                }
                Console.WriteLine("");
            }
            return Task.CompletedTask;
        }
    }
}
