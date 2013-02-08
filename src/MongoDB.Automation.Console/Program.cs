using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoDB.Automation.Console
{
    public static class Program
    {
        static void Main(string[] args)
        {
            //args = new string[] { "--replSet", "rs0", "--dbpath", "c:\\data\\db\\{replSet}\\{port}", "--ports", "40000,40001,40002" };

            var result = GetArguments(args);

            bool startClean = !result.ContainsKey("restart");
            result.Remove("restart");

            string binDir;
            if (!result.TryGetValue("binDir", out binDir))
            {
                var binDirs = GetBinDirectories();
                string binVersion;
                if (!result.TryGetValue("binVersion", out binVersion))
                {
                    binVersion = "default";
                }

                binDir = binDirs[binVersion];
            }

            var controllerConfig = new ConfigurationFactory(binDir, result).GetConfiguration();

            new Automate().From(controllerConfig).Start(startClean ? StartOptions.Clean : StartOptions.None);
        }

        private static Dictionary<string, string> GetArguments(IEnumerable<string> args)
        {
            var result = new Dictionary<string, string>();
            string name = null;

            foreach (var arg in args)
            {
                if (arg.StartsWith("-"))
                {
                    if (name != null)
                    {
                        result.Add(name, null);
                        name = null;
                    }

                    if (arg.StartsWith("--"))
                    {
                        name = arg.Substring(2);
                    }
                    else
                    {
                        name = arg.Substring(1);
                    }
                }
                else
                {
                    result.Add(name, arg);
                    name = null;
                }
            }

            return result;
        }

        private static Dictionary<string, string> GetBinDirectories()
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            foreach (var key in ConfigurationManager.AppSettings.AllKeys)
            {
                if (key.StartsWith("mongodb."))
                {
                    result.Add(key.Substring(8), ConfigurationManager.AppSettings[key]);
                }
            }

            return result;
        }
    }
}