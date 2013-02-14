using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoDB.Automation.Console.CommandLine
{
    public class ParserContext
    {
        private readonly object _result;
        private readonly List<Token> _tokens;

        public ParserContext(IEnumerable<Token> tokens, object result)
        {
            if (tokens == null)
            {
                throw new ArgumentNullException("tokens");
            }
            if (result == null)
            {
                throw new ArgumentNullException("result");
            }

            _tokens = tokens.ToList();
            _result = result;
        }

        public object Result
        {
            get { return _result; }
        }

        public int TokenCount
        {
            get { return _tokens.Count; }
        }

        public Token LA(int count)
        {
            return _tokens[count];
        }

        public IEnumerable<Token> ReadAll()
        {
            return _tokens;
        }

        public ParserContext Skip(int count)
        {
            return new ParserContext(_tokens.Skip(count), _result);
        }
    }
}