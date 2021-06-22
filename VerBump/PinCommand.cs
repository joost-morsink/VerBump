using CommandLine;
using System.Threading.Tasks;

namespace VerBump
{
    [Verb("pin")]
    public class PinCommand : BaseCommand
    {
        protected override Task Execute()
        {
            Config.Base = Repo.Head.Tip.Sha;
            return SaveConfig();
        }
    }
}
