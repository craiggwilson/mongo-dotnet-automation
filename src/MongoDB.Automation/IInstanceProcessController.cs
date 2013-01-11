using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MongoDB.Automation
{
    public interface IInstanceProcessController
    {
        /// <summary>
        /// Starts all the instances.
        /// </summary>
        void Start();

        /// <summary>
        /// Stops all the instances.
        /// </summary>
        void Stop();

        /// <summary>
        /// Waits up to the specified timeout for all the instances to respond to a connection attempt.
        /// </summary>
        /// <param name="timeout"></param>
        void WaitForAvailability(TimeSpan timeout);
    }
}