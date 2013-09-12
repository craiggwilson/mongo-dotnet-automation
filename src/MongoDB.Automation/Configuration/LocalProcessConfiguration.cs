using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MongoDB.Automation.Configuration
{
    public class LocalProcessConfiguration : IProcessConfiguration
    {
        private readonly string _executablePath;
        private readonly IEnumerable<KeyValuePair<string, string>> _arguments;

        public LocalProcessConfiguration(string executablePath)
            : this(executablePath, new Dictionary<string,string>())
        { }

        public LocalProcessConfiguration(string executablePath, IEnumerable<KeyValuePair<string, string>> arguments)
        {
            _executablePath = executablePath;
            _arguments = arguments ?? new Dictionary<string, string>();
        }

        public IEnumerable<KeyValuePair<string, string>> Arguments
        {
            get { return _arguments; }
        }

        public string ExecutablePath
        {
            get { return _executablePath; }
        }
    }
}