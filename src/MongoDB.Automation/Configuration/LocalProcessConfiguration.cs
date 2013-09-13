using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MongoDB.Automation.Configuration
{
    public class LocalProcessConfiguration : IProcessConfiguration
    {
        private string _executablePath;
        private IEnumerable<KeyValuePair<string, string>> _arguments;

        public LocalProcessConfiguration()
        {
            _arguments = new Dictionary<string, string>();
        }

        public IEnumerable<KeyValuePair<string, string>> Arguments
        {
            get { return _arguments; }
            set { _arguments = value; }
        }

        public string ExecutablePath
        {
            get { return _executablePath; }
            set { _executablePath = value; }
        }

        public void Validate()
        {
            if (_executablePath == null)
            {
                throw new ArgumentNullException("ExecutablePath");
            }
            if (_arguments == null)
            {
                throw new ArgumentNullException("Arguments");
            }
        }
    }
}