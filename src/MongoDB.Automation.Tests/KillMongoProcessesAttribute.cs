using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace MongoDB.Automation
{
    [AttributeUsage(AttributeTargets.Method)]
    public class KillMongoProcessesAttribute : Attribute, ITestAction
    {
        public ActionTargets Targets
        {
            get { return ActionTargets.Default; }
        }

        public void AfterTest(TestDetails testDetails)
        {
            KillMongoProcesses();
        }

        public void BeforeTest(TestDetails testDetails)
        {
            KillMongoProcesses();
        }

        private static void KillMongoProcesses()
        {
            var processes = Process.GetProcessesByName("mongod");
            foreach (var process in processes)
            {
                process.Kill();
            }
        }
    }
}