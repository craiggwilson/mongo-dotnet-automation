using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoDB.Automation
{
    public static class TestConfiguration
    {
        public static string GetMongodPath()
        {
            return Path.Combine(
                GetBinPath(), 
                "mongod.exe");
        }

        private static string GetBinPath()
        {
            return ConfigurationManager.AppSettings.Get("mongodb.binpath");
        }
    }
}