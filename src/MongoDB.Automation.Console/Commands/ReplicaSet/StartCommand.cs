using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoDB.Automation.Console.Commands.ReplicaSet
{
    internal class StartCommand : ReplicaSetVerbCommand
    {
        public override string Name
        {
            get { return "Start"; }
        }

        protected override void Execute()
        {
            var config = GetConfiguration();

            new ReplicaSetController(config, GetProcessFactory()).Start(StartOptions.None);
        }
    }
}