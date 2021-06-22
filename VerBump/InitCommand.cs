using System.Linq;
using System.Collections.Generic;
using CommandLine;
using System.Threading.Tasks;

namespace VerBump
{
    [Verb("init")]
    public class InitCommand : BaseCommand
    {
        private static readonly string[] SUPPORTED = new[] { "cs" };
        [Option('t', "types", Required = false, HelpText = "The type of projects. Defaults to cs. Supported are: cs")]
        public IEnumerable<string> Types { get; set; }

        protected async override Task Execute()
        {
            if (Types != null && Types.Any())
                Config.Types = Types.Where(SUPPORTED.Contains).ToArray();
            await SaveConfig();
        }
    }
}
