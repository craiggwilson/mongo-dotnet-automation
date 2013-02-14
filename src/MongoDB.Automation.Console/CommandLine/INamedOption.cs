using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoDB.Automation.Console.CommandLine
{
    public interface INamedOption : IOption
    {
        IEnumerable<string> Names { get; }
    }
}