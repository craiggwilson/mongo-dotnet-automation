using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoDB.Automation.Console.CommandLine
{
    public interface IParser<T>
    {
        T Parse(IEnumerable<string> args);
    }
}