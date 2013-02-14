using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoDB.Automation.Console.CommandLine
{
    public enum TokenType
    {
        Name,
        Value
    }

    public abstract class Token
    {
        private readonly TokenType _type;

        protected Token(TokenType type)
        {
            _type = type;
        }

        public TokenType Type
        {
            get { return _type; }
        }

        public static NameToken Name(string name)
        {
            return new NameToken(name);
        }

        public static ValueToken Value(string value)
        {
            return new ValueToken(value);
        }
    }

    public sealed class NameToken : Token
    {
        private readonly string _name;

        public NameToken(string name)
            : base(TokenType.Name)
        {
            _name = name;
        }

        public string Name
        {
            get { return _name; }
        }
    }

    public sealed class ValueToken : Token
    {
        private readonly string _value;

        public ValueToken(string value)
            : base(TokenType.Value)
        {
            _value = value;
        }

        public string Value
        {
            get { return _value; }
        }
    }
}