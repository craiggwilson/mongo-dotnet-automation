using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MongoDB.Automation.Local
{
    public interface ILocalInstanceProcessConfiguration
    {
        string BinPath { get; }

        IEnumerable<KeyValuePair<string, string>> Arguments { get; }
    }
}