using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoDB.Automation.Console.CommandLine
{
    public class NameValueOption<T> : INamedOption
    {
        private readonly IEnumerable<string> _names;
        private readonly bool _required;
        private readonly Action<string, string, T> _setter;

        public NameValueOption(string name, bool required, Action<string, string, T> setter)
            : this(new [] { name }, required, setter)
        { }

        public NameValueOption(IEnumerable<string> names, bool required, Action<string, string, T> setter)
        {
            _names = names;
            _required = required;
            _setter = setter;
        }

        public IEnumerable<string> Names
        {
            get { return _names; }
        }
        public bool IsRequired
        {
            get { return _required; }
        }

        public ParserContext Handle(ParserContext context)
        {
            var current = context.LA(0) as NameToken;
            if (current != null && context.TokenCount > 1 && _names.Contains(current.Name))
            {
                var value = context.LA(1) as ValueToken;
                if (value != null)
                {
                    _setter(current.Name, value.Value, (T)context.Result);
                    return context.Skip(2);
                }
            }

            return context;
        }
    }
}