using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MongoDB.Automation
{
    public abstract class AbstractInstanceProcess<TSettings> : IInstanceProcess<TSettings>
        where TSettings : IInstanceProcessSettings
    {
        private readonly TSettings _settings;

        public AbstractInstanceProcess(TSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException("settings");
            }

            _settings = settings;
        }

        public abstract MongoServerAddress Address { get; }

        public abstract bool IsRunning { get; }

        public TSettings Settings
        {
            get { return _settings; }
        }

        public MongoServer Connect()
        {
            return Connect(TimeSpan.FromMinutes(1));
        }

        public MongoServer Connect(TimeSpan timeout)
        {
            var client = new MongoClient(string.Format("mongodb://{0}/?safe=true&slaveOk=true", Address));
            var server = client.GetServer();
            server.Connect(timeout);
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

            Util.Timeout(timeout,
                string.Format("Unable to connect to instance for address {0}", Address),
                TimeSpan.FromSeconds(10),
                remaining =>
                {
                    Connect(remaining);
                    return true;
                });
        }
    }
}