using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoDB.Automation.Console.Commands
{
    internal abstract class ParsingCommand : ICommand
    {
        public abstract string Name { get; }

        public Dictionary<string, string> UnboundArguments { get; private set; } 

        public void Execute(string[] args)
        {
            Parse(args);
            Execute();
        }

        protected abstract void Execute();

        private void Parse(IEnumerable<string> args)
        {
            var options = (from p in this.GetType().GetProperties()
                           from a in p.GetCustomAttributes(true).OfType<OptionAttribute>()
                           select new { ShortName = a.ShortName, LongName = a.LongName ?? p.Name, DefaultValue = a.DefaultValue, Property = p }).ToList();

            var result = new Dictionary<string, string>();
            string name = null;

            foreach (var arg in args)
            {
                if (arg.StartsWith("-"))
                {
                    if (name != null)
                    {
                        result.Add(name, null);
                        name = null;
                    }

                    if (arg.StartsWith("--"))
                    {
                        name = arg.Substring(2);
                    }
                    else
                    {
                        name = arg.Substring(1);
                    }
                }
                else
                {
                    var matched = options.FirstOrDefault(x => 
                        (x.ShortName != null && x.ShortName.ToString().Equals(arg, StringComparison.InvariantCultureIgnoreCase)) 
                        || x.LongName.Equals(arg, StringComparison.InvariantCultureIgnoreCase));

                    if (matched != null)
                    {
                        matched.Property.SetValue(this, ConvertTo(arg, matched.Property.PropertyType));
                        options.Remove(matched);
                    }
                    else
                    {
                        result.Add(name, arg);
                    }
                    name = null;
                }
            }

            foreach (var unmatchedOption in options)
            {
                if (unmatchedOption.DefaultValue != null)
                {
                    unmatchedOption.Property.SetValue(this, ConvertTo(unmatchedOption.DefaultValue, unmatchedOption.Property.PropertyType));
                }
            }

            UnboundArguments = result;
        }

        private object ConvertTo(string value, Type type)
        {
            return Convert.ChangeType(value, type);
        }
    }
}