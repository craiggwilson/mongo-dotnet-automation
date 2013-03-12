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
            args = new [] 
            { 
                "start", 
                "--replSet", "testing", 
                "--dbpath", @"c:\MongoDB\{replSet}\{port}",
                "--ports", "40000,40001,40002" 
            };

            string verb = null;
            if (args.Length > 0)
            {
                verb = args[0];
                args = args.Skip(1).ToArray();
            }

            if (verb == null || verb.StartsWith("--"))
            {
                throw new InvalidOperationException("Must begin with a valid verb.");
            }

            var result = GetArguments(args);
            var binDir = GetBinDirectory(result);

            if (verb == "start" || verb == "restart")
            {
                new StartCommand(verb, binDir, result).Run();
            }
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

        private static string GetBinDirectory(Dictionary<string, string> result)
        {
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
            else
            {
                result.Remove("binDir");
            }
            return binDir;
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