using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
                           select new Option
                           {
                               Short = a.ShortName.ToString(),
                               Long = a.LongName ?? p.Name,
                               DefaultValue = a.DefaultValue,
                               Required = a.Required,
                               Property = p
                           }).ToList();

            var result = new Dictionary<string, string>();
            string name = null;
            List<string> values = null;

            foreach (var arg in args)
            {
                if (arg.StartsWith("-"))
                {
                    // we are done with the previous option and it didn't contain a value
                    if (name != null)
                    {
                        var matched = options.FirstOrDefault(o => o.Matches(name));
                        if(matched != null)
                        {
                            SetProperty(matched, values == null ? null : values.ToArray());
                            options.Remove(matched);
                        }
                        else
                        {
                            result.Add(name, values == null ? null : string.Join(",", values));
                        }
                        name = null;
                        values = null;
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
                else if (string.IsNullOrEmpty(name))
                {
                    throw new InvalidOperationException(string.Format("Argument {0} is unable to be processed.", arg));
                }
                else
                {
                    if (values == null)
                    {
                        values = new List<string>();
                    }
                    values.Add(arg);
                }
            }

            if (name != null)
            {
                var matched = options.FirstOrDefault(o => o.Matches(name));
                if (matched != null)
                {
                    SetProperty(matched, values == null ? null : values.ToArray());
                    options.Remove(matched);
                }
                else
                {
                    result.Add(name, values == null ? null : string.Join(",", values));
                }
                name = null;
                values = null;
            }

            foreach (var unmatchedOption in options)
            {
                if (unmatchedOption.Required)
                {
                    throw new InvalidOperationException(string.Format("Option {0} is required.", unmatchedOption.Long));
                }
                if (unmatchedOption.DefaultValue != null)
                {
                    SetProperty(unmatchedOption, new[] { unmatchedOption.DefaultValue });
                }
            }

            UnboundArguments = result;
        }

        private object ConvertTo(string value, Type type)
        {
            return Convert.ChangeType(value, type);
        }

        private void SetProperty(Option option, string[] values)
        {
            var type = GetElementType(option.Property.PropertyType);

            if (values == null || values.Length == 0)
            {
                if (option.Property.PropertyType != typeof(bool) && option.Property.PropertyType != typeof(bool?))
                {
                    throw new InvalidOperationException(string.Format("Option {0} must have a value.", option.Long));
                }

                option.Property.SetValue(this, true);
            }
            else if (values.Length == 1)
            {
                if (!option.Property.PropertyType.IsArray)
                {
                    option.Property.SetValue(this, ConvertTo(values[0], option.Property.PropertyType));
                }
                else
                {
                    values = values[0].Split(new [] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    var setValues = values.Select(v => ConvertTo(v, type)).ToArray();
                    var array = Array.CreateInstance(type, setValues.Length);
                    setValues.CopyTo(array, 0);
                    option.Property.SetValue(this, array);
                }
            }
            else
            {
                if (!option.Property.PropertyType.IsArray)
                {
                    throw new InvalidOperationException(string.Format("Option {0} was provided with more than one value.", option.Long));
                }
                else
                {
                    var setValues = values.Select(v => ConvertTo(v, type)).ToArray();
                    var array = Array.CreateInstance(type, setValues.Length);
                    setValues.CopyTo(array, 0);
                    option.Property.SetValue(this, array);
                }
            }
        }

        private static Type GetElementType(Type type)
        {
            if (type.IsArray)
            {
                return type.GetElementType();
            }

            return type;
        }

        private class Option
        {
            public string Short;
            public string Long;
            public string DefaultValue;
            public bool Required;
            public PropertyInfo Property;

            public bool Matches(string name)
            {
                if (Short != null && Short.Equals(name, StringComparison.InvariantCultureIgnoreCase))
                {
                    return true;
                }

                return Long.Equals(name, StringComparison.InvariantCultureIgnoreCase);
            }
        }
    }
}