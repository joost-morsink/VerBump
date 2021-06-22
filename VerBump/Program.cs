using System;
using LibGit2Sharp;
using System.Linq;
using System.Xml.Linq;
using CommandLine;
using System.Reflection;

namespace VerBump
{
    class Program
    {
        static void Main(string[] args)
        {
            var commands = (from t in Assembly.GetExecutingAssembly().ExportedTypes
                            where t.GetCustomAttributes<VerbAttribute>().Any()
                            select t).ToArray();

            Parser.Default.ParseArguments(args, commands).WithParsed(opts =>
            {
                ((IExecutable)opts).Run().Wait();
            });
        }
    }
}
