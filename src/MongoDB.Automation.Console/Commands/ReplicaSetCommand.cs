using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Automation.Console.Commands.ReplicaSet;

namespace MongoDB.Automation.Console.Commands
{
    internal class ReplicaSetCommand : ICommand
    {
        public string Name
        {
            get { return "ReplicaSet"; }
        }

        public void Execute(string[] args)
        {
            new Commander()
                .AddCommand<StartCommand>()
                .AddCommand<RestartCommand>()
                .Execute(args);
        }
    }
}