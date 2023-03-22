using System;
using LibGit2Sharp;
using System.Linq;
using System.Collections.Generic;
using CommandLine;
using System.Threading.Tasks;

namespace VerBump
{
    [Verb("bump", HelpText = "Bumps all projects to next version, if they have changes compared to the base.")]
    public class BumpCommand : BaseCommand
    {
        [Option('t', "type", Default = Change.Minor, HelpText = "Type of bump: Patch, Minor, Major.")]
        public Change Change { get; set; }
        protected override Task Execute()
        {
            foreach (var (name, finder) in Config.Types.Select(t => (t, t.GetVersionFinder())))
            {
                var fc = Console.ForegroundColor;
                Console.WriteLine($"Finder {name}:");
                var baseCommitSha = Config.Base == null ? null : Repo.Tags[Config.Base]?.PeeledTarget.Peel<Commit>().Sha ?? Config.Base;
                var baseCommit = Config.Base == null ? null : Repo.Lookup<Commit>(baseCommitSha);
                var baseFs = new GitFileSystem(Repo, baseCommit);
                var baseVers = finder.GetVersions(baseFs);
                var fs = new GitWorkTreeFileSystem(Repository);
                var changes = new HashSet<string>(finder.Changed(baseFs, fs));
                foreach (var ver in finder.GetVersions(fs).OrderBy(x => x.Key))
                {
                    if (baseVers.TryGetValue(ver.Key, out var baseVer))
                    {
                        if (changes.Contains(ver.Key) && baseVer.Compare(ver.Value) < this.Change)
                        {
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine($"Bumping {ver.Key}: {ver.Value} => {baseVer.Bump(this.Change)}");
                            finder.SetVersion(fs.Navigate(ver.Key), baseVer.Bump(this.Change));
                        }
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.DarkGreen;
                        Console.WriteLine($"{ver.Key}: {ver.Value} !! NEW !!");
                    }
                }
                Console.WriteLine("");
                Console.ForegroundColor = fc;
            }
            return Task.CompletedTask;
        }

    }
}
