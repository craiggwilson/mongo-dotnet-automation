using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Automation.Configuration;

namespace MongoDB.Automation
{
    public interface IProcess
    {
        /// <summary>
        /// Gets the address for the instance.
        /// </summary>
        MongoServerAddress Address { get; }

        /// <summary>
        /// Indicates whether the process is currently running.
        /// </summary>
        bool IsRunning { get; }

        /// <summary>
        /// Connects to the instance.
        /// </summary>
        /// <returns>A connected MongoServer.</returns>
        MongoServer Connect();

        /// <summary>
        /// Creates a MongoServer and connects to it.
        /// </summary>
        /// <returns>A connected MongoServer.</returns>
        MongoServer Connect(TimeSpan timeout);

        /// <summary>
        /// Gets the configuration.
        /// </summary>
        /// <returns>The configuration for the process.</returns>
        IProcessConfiguration GetConfiguration();

        /// <summary>
        /// Runs a command against the admin database.
        /// </summary>
        /// <param name="commandName"></param>
        /// <returns>The result of the command.</returns>
        CommandResult RunAdminCommand(string commandName);

        /// <summary>
        /// Runs a command against the admin database.
        /// </summary>
        /// <param name="commandName"></param>
        /// <returns>The result of the command.</returns>
        CommandResult RunAdminCommand(CommandDocument commandDocument);

        /// <summary>
        /// Starts the process if it is not running.  Otherwise, it does nothing.
        /// </summary>
        /// <param name="options">The options.</param>
        void Start(StartOptions options);

        /// <summary>
        /// Stops the process if it is running.  Otherwise, it does nothing.
        /// </summary>
        void Stop();

        /// <summary>
        /// Waits up to the specified timeout for the instance to respond to a connect attempt.
        /// </summary>
        /// <param name="timeout"></param>
        void WaitForAvailability(TimeSpan timeout);
    }
}