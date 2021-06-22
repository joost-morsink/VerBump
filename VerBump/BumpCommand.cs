using System;
using LibGit2Sharp;
using System.Linq;
using System.Collections.Generic;
using CommandLine;
using System.Threading.Tasks;

namespace VerBump
{
    [Verb("bump")]
    public class BumpCommand : BaseCommand
    {
        [Option('t', "type", Default = Change.Minor)]
        public Change Change { get; set; }
        protected override Task Execute()
        {
            foreach (var (name, finder) in Config.Types.Select(t => (t, t.GetVersionFinder())))
            {

                Console.WriteLine($"Finder {name}:");
                var baseCommit = Config.Base == null ? null : Repo.Lookup<Commit>(Config.Base);
                var baseFs = new GitFileSystem(Repo, baseCommit);
                var baseVers = finder.GetVersions(baseFs);
                var fs = new GitFileSystem(Repo, Repo.Head.Tip);
                var disk = new FileSystem(Repository);
                var changes = new HashSet<string>(finder.Changed(baseFs, fs));
                foreach (var ver in finder.GetVersions(fs).OrderBy(x => x.Key))
                {
                    if (baseVers.TryGetValue(ver.Key, out var baseVer) && changes.Contains(ver.Key))
                    {
                        if (baseVer.Compare(ver.Value) < this.Change)
                        {
                            Console.WriteLine($"Bumping {ver.Key}: {ver.Value} => {baseVer.Bump(this.Change)}");
                            finder.SetVersion(disk.Navigate(ver.Key), baseVer.Bump(this.Change));
                        }
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
