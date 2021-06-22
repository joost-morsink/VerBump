using System.Collections.Generic;

namespace VerBump
{
    public class Config
    {
        public string[] Types { get; set; }
        public string Base { get; set; }
        public string BuildNumberTemplate { get; set; }
        public Dictionary<string, string> Versions { get; set; }
    }
}
