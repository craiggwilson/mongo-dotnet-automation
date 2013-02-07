using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MongoDB.Automation.Configuration
{
    public interface ILocalInstanceProcessConfiguration : IInstanceProcessConfiguration
    {
        IEnumerable<KeyValuePair<string, string>> Arguments { get; }

        string BinPath { get; }
    }
}