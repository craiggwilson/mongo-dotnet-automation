using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MongoDB.Automation.Local
{
    public class LocalInstanceProcessConfiguration : ILocalInstanceProcessConfiguration
    {
        private readonly string _binPath;
        private readonly IEnumerable<KeyValuePair<string, string>> _arguments;

        public LocalInstanceProcessConfiguration(string binPath)
            : this(binPath, new Dictionary<string,string>())
        { }

        public LocalInstanceProcessConfiguration(string binPath, IEnumerable<KeyValuePair<string,string>> arguments)
        {
            _binPath = binPath;
            _arguments = arguments ?? new Dictionary<string, string>();
        }

        public string BinPath
        {
            get { return _binPath; }
        }

        public IEnumerable<KeyValuePair<string, string>> Arguments
        {
            get { return _arguments; }
        }
    }
}
