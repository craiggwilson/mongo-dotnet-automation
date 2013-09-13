using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Automation.Console.Commands.Standalone;

namespace MongoDB.Automation.Console.Commands
{
    internal class StandaloneCommand : ICommand
    {
        public string Name
        {
            get { return "Standalone"; }
        }

        public void Execute(string[] args)
        {
            new Commander()
                .AddCommand<StartCommand>()
                .Execute(args);
        }
    }
}