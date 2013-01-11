using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MongoDB.Driver
{
    internal static class Extensions
    {
        public static bool IsLocal(this MongoServerAddress address)
        {
            if (address.Host.Equals("localhost", StringComparison.InvariantCultureIgnoreCase))
            {
                return true;
            }

            if (address.Host.Equals("127.0.0.1"))
            {
                return true;
            }

            if (address.Host.Equals(Environment.MachineName, StringComparison.InvariantCultureIgnoreCase))
            {
                return true;
            }

            return false;
        }
    }
}