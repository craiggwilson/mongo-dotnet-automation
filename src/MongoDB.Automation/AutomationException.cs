using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MongoDB.Automation
{
    [Serializable]
    public class AutomationException : Exception
    {
        public AutomationException() { }
        public AutomationException(string message) : base(message) { }
        public AutomationException(string message, Exception inner) : base(message, inner) { }
        protected AutomationException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}
