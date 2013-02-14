using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoDB.Automation.Console.CommandLine
{
    [Serializable]
    public class MissingCommandLineOptionException : CommandLineException
    {
        public MissingCommandLineOptionException() { }
        public MissingCommandLineOptionException(string message) : base(message) { }
        public MissingCommandLineOptionException(string message, Exception inner) : base(message, inner) { }
        protected MissingCommandLineOptionException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}
