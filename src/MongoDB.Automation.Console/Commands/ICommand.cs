using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoDB.Automation.Console.Commands
{
    internal interface ICommand
    {
        string Name { get; }

        void Execute(string[] args);
    }
}