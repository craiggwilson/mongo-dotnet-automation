using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Automation.Configuration;
using MongoDB.Driver;

namespace MongoDB.Automation
{
    public interface IController
    {
        /// <summary>
        /// Gets the configuration.
        /// </summary>
        /// <returns>The configuration for the controller.</returns>
        IControllerConfiguration GetConfiguration();

        /// <summary>
        /// Starts all the instances.
        /// </summary>
        /// <param name="options">The options.</param>
        void Start(StartOptions options);

        /// <summary>
        /// Stops all the instances.
        /// </summary>
        void Stop();

        /// <summary>
        /// Waits up to the specified timeout for all the instances to respond to a connection attempt.
        /// </summary>
        /// <param name="timeout"></param>
        void WaitForFullAvailability(TimeSpan timeout);
    }
}