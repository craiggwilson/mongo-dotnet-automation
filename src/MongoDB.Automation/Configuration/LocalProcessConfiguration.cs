using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MongoDB.Automation.Configuration
{
    public class LocalProcessConfiguration : ILocalProcessConfiguration
    {
        private readonly string _binPath;
        private readonly IEnumerable<KeyValuePair<string, string>> _arguments;

        public LocalProcessConfiguration(string binPath)
            : this(binPath, new Dictionary<string,string>())
        { }

        public LocalProcessConfiguration(string binPath, IEnumerable<KeyValuePair<string,string>> arguments)
        {
            _binPath = binPath;
            _arguments = arguments ?? new Dictionary<string, string>();
        }

        public IEnumerable<KeyValuePair<string, string>> Arguments
        {
            get { return _arguments; }
        }

        public string BinPath
        {
            get { return _binPath; }
        }
    }
}