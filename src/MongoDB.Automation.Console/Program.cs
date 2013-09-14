using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Automation.Console.Commands;

namespace MongoDB.Automation.Console
{
    public static class Program
    {
        static void Main(string[] args)
        {
            Config.SetError(System.Console.Error);
            Config.SetOut(System.Console.Out);

            args = new[] { "replicaset", "start", "-wait" };

            new Commander()
                .AddDefaultCommand<StandaloneCommand>()
                .AddCommand<ReplicaSetCommand>()
                .Execute(args);
        }
    }
}