using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MongoDB.Automation.Console.Commands
{
    [AttributeUsage(AttributeTargets.Property)]
    internal class OptionAttribute : Attribute
    {
        public string DefaultValue { get; set; }

        public char ShortName { get; set; }

        public string LongName { get; set; }
    }
}