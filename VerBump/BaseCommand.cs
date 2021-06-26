using System;
using LibGit2Sharp;
using CommandLine;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace VerBump
{
    public abstract class BaseCommand : IExecutable
    {
        protected BaseCommand()
        {
            _repo = new Lazy<Repository>(() => new Repository(Repository));
        }
        [Option('r', "repo", Default = ".", HelpText = "Sets the repository location.")]
        public string Repository { get; set; }
        public Config Config { get; set; }
        private Lazy<Repository> _repo;
        protected Repository Repo => _repo.Value;
        protected async Task LoadConfig()
        {
            var configPath = Path.Combine(Repository, ".verbump");
            if (!File.Exists(configPath))
            {
                Config = new Config
                {
                    Base = null,
                    Types = new[] { "cs" }
                };
                await SaveConfig();
            }
            using var str = File.OpenRead(configPath);
            Config = await JsonSerializer.DeserializeAsync<Config>(str);
        }
        protected async Task SaveConfig()
        {
            var configPath = Path.Combine(Repository, ".verbump");
            using var str = File.Create(configPath);
            await JsonSerializer.SerializeAsync(str, Config);
        }
        public async Task Run()
        {
            await LoadConfig();
            await Execute();
        }
        protected abstract Task Execute();
    }
}
