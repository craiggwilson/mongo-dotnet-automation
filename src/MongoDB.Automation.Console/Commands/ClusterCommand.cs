using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoDB.Automation.Console.Commands
{
    internal abstract class ClusterCommand : ParsingCommand
    {
        [Option(LongName="bin", DefaultValue="default")]
        public string Binaries { get; set; }

        protected string GetBinaryPath()
        {
            var binDirs = GetBinDirectories();
            string binDir;
            if (binDirs.TryGetValue(Binaries, out binDir))
            {
                return binDir;
            }

            // it might be a raw path specification...
            return Binaries;
        }

        protected IProcessFactory GetProcessFactory()
        {
            return new DefaultProcessFactory();
        }

        private static Dictionary<string, string> GetBinDirectories()
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            foreach (var key in ConfigurationManager.AppSettings.AllKeys)
            {
                if (key.StartsWith("binaries."))
                {
                    result.Add(key.Substring(9), ConfigurationManager.AppSettings[key]);
                }
            }

            return result;
        }
    }
}