using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoDB.Automation.Console.CommandLine
{
    public class FlagOption<T> : INamedOption
    {
        private readonly IEnumerable<string> _names;
        private readonly Action<string, T> _setter;

        public FlagOption(string name, Action<string, T> setter)
            : this(new [] { name }, setter)
        { }

        public FlagOption(IEnumerable<string> names, Action<string, T> setter)
        {
            _names = names;
            _setter = setter;
        }

        public IEnumerable<string> Names
        {
            get { return _names; }
        }

        public bool IsRequired
        {
            get { return false; }
        }

        public ParserContext Handle(ParserContext context)
        {
            var current = context.LA(0) as NameToken;
            if (current != null && _names.Contains(current.Name))
            {
                _setter(current.Name, (T)context.Result);
                return context.Skip(1);
            }

            return context;
        }
    }
}