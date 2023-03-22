using CommandLine;
using System.Threading.Tasks;

namespace VerBump
{
    [Verb("pin", HelpText = "Pins a new base.")]
    public class PinCommand : BaseCommand
    {
        [Value(0, Required = false, HelpText = "The reference to pin the base to. Either a sha or a tag.")]
        public string Spec { get; set; }
        protected override Task Execute()
        {
            Config.Base = Spec ?? Repo.Head.Tip.Sha;
            return SaveConfig();
        }
    }
}
