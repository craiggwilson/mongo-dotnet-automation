using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MongoDB.Automation
{
    public abstract class AbstractInstanceProcess : IInstanceProcess
    {
        public abstract MongoServerAddress Address { get; }

        public abstract bool IsRunning { get; }

        public MongoServer Connect()
        {
            return Connect(TimeSpan.FromMinutes(1));
        }

        public MongoServer Connect(TimeSpan timeout)
        {
            if (!IsRunning)
            {
                throw new AutomationException("Cannot connect to an instance that is not running.");
            }

            var client = new MongoClient(string.Format("mongodb://{0}/?safe=true&slaveOk=true", Address));
            var server = client.GetServer();
            Retry.WithTimeout(
                remaining =>
                {
                    server.Connect(remaining);
                    return true;
                },
                timeout,
                TimeSpan.FromSeconds(5));

            return server;
        }

        public CommandResult RunAdminCommand(string commandName)
        {
            return RunAdminCommand(new CommandDocument(commandName, 1));
        }

        public CommandResult RunAdminCommand(CommandDocument commandDocument)
        {
            Config.Out.WriteLine("Sending admin command to {0}: {1}", Address, commandDocument.ToJson());
            return Connect()
                .GetDatabase("admin")
                .RunCommand(commandDocument);
        }

        public abstract void Start(StartOptions options);

        public abstract void Stop();

        public void WaitForAvailability(TimeSpan timeout)
        {
            Config.Out.WriteLine("Waiting for mongod at address {0} to become available.", Address);

            Retry.WithTimeout(
                remaining => 
                {
                    Connect(remaining);
                    return true;
                },
                timeout,
                TimeSpan.FromSeconds(10));
        }
    }
}