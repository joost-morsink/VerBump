using System;
using LibGit2Sharp;
using System.Linq;
using System.Collections.Generic;
using CommandLine;
using System.Threading.Tasks;

namespace VerBump
{
    [Verb("info", HelpText = "Displays info about projects and whether there have been changes or bumps since the pinned base.")]
    public class InfoCommand : BaseCommand
    {
        protected override Task Execute()
        {
            foreach (var (name, finder) in Config.Types.Select(t => (t, t.GetVersionFinder())))
            {
                var fc = Console.ForegroundColor;
                Console.WriteLine($"Finder {name}:");
                var baseCommitSha = Repo.Tags[Config.Base]?.PeeledTarget.Peel<Commit>().Sha ?? Config.Base;
                var baseCommit = Config.Base == null ? null : Repo.Lookup<Commit>(baseCommitSha);
                var baseFs = new GitFileSystem(Repo, baseCommit);
                var baseVers = finder.GetVersions(baseFs);
                var fs = new GitWorkTreeFileSystem(Repository);
                var changes = new HashSet<string>(finder.Changed(baseFs, fs));
                foreach (var ver in finder.GetVersions(fs).OrderBy(x => x.Key))
                {
                    if (baseVers.TryGetValue(ver.Key, out var baseVer))
                    {
                        if (ver.Value != baseVer)
                        {
                            Console.ForegroundColor = ConsoleColor.DarkGreen;
                            Console.WriteLine($"{ver.Key}: {baseVer} => {ver.Value} ({baseVer.Compare(ver.Value)})");
                        }
                        else if (changes.Contains(ver.Key))
                        {
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine($"{ver.Key}: {ver.Value} !! CHANGED !!");
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Gray;
                            Console.WriteLine($"{ver.Key}: {ver.Value}");
                        }
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Blue;
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
