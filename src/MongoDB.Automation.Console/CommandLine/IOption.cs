using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoDB.Automation.Console.CommandLine
{
    public interface IOption
    {
        bool IsRequired { get; }

        ParserContext Handle(ParserContext context);
    }
}