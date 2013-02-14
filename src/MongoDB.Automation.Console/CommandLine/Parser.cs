using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoDB.Automation.Console.CommandLine
{
    public class Parser<T> : IParser<T> where T : new()
    {
        private IEnumerable<string> _argNamePrefixes;
        private List<IOption> _options;

        public Parser()
        {
            _argNamePrefixes = new[] { "--", "-" };
            _options = new List<IOption>();
        }

        public Parser<T> NamedArgsArePrefixedWith(params string[] prefixes)
        {
            _argNamePrefixes = prefixes.OrderByDescending(x => x.Length);
            return this;
        }

        public T Parse(IEnumerable<string> args)
        {
            var context = new ParserContext(Tokenize(args), new T());

            foreach (var option in _options)
            {
                option.Handle(context);
            }

            return (T)context.Result;
        }

        private IEnumerable<Token> Tokenize(IEnumerable<string> args)
        {
            foreach (var arg in args)
            {
                var prefix = _argNamePrefixes.FirstOrDefault(x => arg.StartsWith(x));
                if (prefix != null)
                {
                    yield return Token.Name(arg.Substring(prefix.Length));
                }
                else
                {
                    yield return Token.Value(arg);
                }
            }
        }
    }
}