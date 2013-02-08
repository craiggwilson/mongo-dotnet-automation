using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MongoDB.Automation.Configuration
{
    public interface ILocalProcessConfiguration : IProcessConfiguration
    {
        IEnumerable<KeyValuePair<string, string>> Arguments { get; }

        string ExecutablePath { get; }
    }
}