using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoDB.Automation.Console.CommandLine
{
    public class ParserConfiguration
    {
        private readonly IEnumerable<string> _argNamePrefixes;

        public ParserConfiguration(IEnumerable<string> argNamePrefixes)
        {
            if (argNamePrefixes == null)
            {
                throw new ArgumentNullException("argNamePrefixes");
            }

            // longest goes first...
            _argNamePrefixes = argNamePrefixes.OrderByDescending(a => a.Length);
        }

        public bool IsOptionName(string value)
        {
            return _argNamePrefixes.Any(x => value.StartsWith(x));
        }

        public string GetOptionName(string value)
        {
            var prefix = _argNamePrefixes.FirstOrDefault(x => value.StartsWith(x));
            if (prefix == null)
            {
                throw new ArgumentException("Is not an option name.  Ensure a call to IsOptionName first.", "value");
            }

            return value.Substring(prefix.Length);
        }

        public bool TryGetOptionName(string value, out string name)
        {
            if (!IsOptionName(value))
            {
                name = null;
                return false;
            }

            name = GetOptionName(value);
            return true;
        }
    }
}