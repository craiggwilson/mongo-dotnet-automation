﻿using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using MongoDB.Automation.Configuration;

namespace MongoDB.Automation
{
    public class StandAloneController : IShardableInstanceProcessController
    {
        private readonly IInstanceProcess _instanceProcess;

        public StandAloneController(IInstanceProcess instanceProcess)
        {
            if (instanceProcess == null)
            {
                throw new ArgumentNullException("instanceProcess");
            }

            _instanceProcess = instanceProcess;
        }

        public MongoServer Connect()
        {
            return Connect(TimeSpan.FromMinutes(1));
        }

        public MongoServer Connect(TimeSpan timeout)
        {
            return _instanceProcess.Connect(timeout);
        }

        public IInstanceProcessControllerConfiguration GetConfiguration()
        {
            throw new NotImplementedException();
        }

        public string GetAddShardAddress()
        {
            return _instanceProcess.Address.ToString();
        }

        public void Start(StartOptions options)
        {
            _instanceProcess.Start(options);
        }

        public void Stop()
        {
            _instanceProcess.Stop();
        }

        public void WaitForFullAvailability(TimeSpan timeout)
        {
            if (!_instanceProcess.IsRunning)
            {
                _instanceProcess.Start(StartOptions.None);
            }

            _instanceProcess.WaitForAvailability(timeout);
        }
    }
}