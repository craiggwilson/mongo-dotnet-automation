using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using MongoDB.Automation.Configuration;

namespace MongoDB.Automation
{
    public class StandAloneController : IShardableController
    {
        private readonly IProcess _process;

        public StandAloneController(StandAloneConfiguration configuration, IProcessFactory processFactory)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException("configuration");
            }
            if (processFactory == null)
            {
                throw new ArgumentNullException("processFactory");
            }
            configuration.Validate();

            _process = processFactory.Create(configuration.Server);
        }

        public MongoServer Connect()
        {
            return Connect(TimeSpan.FromMinutes(1));
        }

        public MongoServer Connect(TimeSpan timeout)
        {
            return _process.Connect(timeout);
        }

        public IControllerConfiguration GetConfiguration()
        {
            return new StandAloneConfiguration { Server = _process.GetConfiguration() };
        }

        public string GetAddShardAddress()
        {
            return _process.Address.ToString();
        }

        public void Start(StartOptions options)
        {
            _process.Start(options);
        }

        public void Stop()
        {
            _process.Stop();
        }

        public void WaitForFullAvailability(TimeSpan timeout)
        {
            if (!_process.IsRunning)
            {
                _process.Start(StartOptions.None);
            }

            _process.WaitForAvailability(timeout);
        }
    }
}