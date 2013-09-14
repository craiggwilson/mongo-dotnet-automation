using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoDB.Automation.Console.Commands.ReplicaSet
{
    internal class StartCommand : ReplicaSetVerbCommand
    {
        [Option]
        public bool Wait { get; set; }

        public override string Name
        {
            get { return "Start"; }
        }

        protected override void Execute()
        {
            var config = GetConfiguration();

            var controller = new ReplicaSetController(config, GetProcessFactory());
            controller.Start(StartOptions.Clean);
            if (Wait)
            {
                controller.WaitForFullAvailability(TimeSpan.FromMinutes(10));
            }
        }
    }
}