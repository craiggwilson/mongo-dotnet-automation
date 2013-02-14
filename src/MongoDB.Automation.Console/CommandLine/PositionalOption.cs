using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoDB.Automation.Console.CommandLine
{
    public class PositionalOption<T> : IOption
    {
        private readonly bool _required;
        private readonly Action<string, T> _setter;

        public PositionalOption(bool required, Action<string, T> setter)
        {
            _required = required;
            _setter = setter;
        }

        public bool IsRequired
        {
            get { return _required; }
        }

        public ParserContext Handle(ParserContext context)
        {
            var token = context.LA(0) as ValueToken;
            if (token != null)
            {
                _setter(token.Value, (T)context.Result);
                return context.Skip(1);
            }

            if (_required)
            {
                throw new MissingCommandLineOptionException("Required positioned argument is missing.");
            }

            return context;
        }
    }
}