using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoDB.Automation.Console.Commands.Standalone
{
    internal class StartCommand : StandAloneVerbCommand
    {
        public override string Name
        {
            get { return "Start"; }
        }

        protected override void Execute()
        {
            var config = GetConfiguration();

            new StandAloneController(config, GetProcessFactory()).Start(StartOptions.Clean);
        }
    }
}