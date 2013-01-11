using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace MongoDB.Automation
{
    public class StandAloneController : IShardableInstanceProcessController
    {
        private readonly IInstanceProcess<StandAloneSettings> _instanceProcess;

        public StandAloneController(IInstanceProcess<StandAloneSettings> instanceProcess)
        {
            if (instanceProcess == null)
            {
                throw new ArgumentNullException("instanceProcess");
            }

            _instanceProcess = instanceProcess;
        }

        public string GetAddShardAddress()
        {
            return _instanceProcess.Address.ToString();
        }

        public void Start()
        {
            _instanceProcess.Start();
        }

        public void Stop()
        {
            _instanceProcess.Stop();
        }

        public void WaitForAvailability(TimeSpan timeout)
        {
            if (!_instanceProcess.IsRunning)
            {
                _instanceProcess.Start();
            }

            _instanceProcess.WaitForAvailability(timeout);
        }
    }
}