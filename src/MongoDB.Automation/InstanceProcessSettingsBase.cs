using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MongoDB.Automation
{
    public abstract class InstanceProcessSettingsBase : IInstanceProcessSettings
    {
        private readonly int _port;

        protected InstanceProcessSettingsBase(int port)
        {
            _port = port;
        }

        public int Port
        {
            get { return _port; }
        }
    }
}
