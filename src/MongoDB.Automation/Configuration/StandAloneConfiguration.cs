using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MongoDB.Automation.Configuration
{
    public class StandAloneConfiguration : IControllerConfiguration
    {
        private IProcessConfiguration _server;

        public IProcessConfiguration Server
        {
            get { return _server; }
            set { _server = value; }
        }

        public void Validate()
        {
            if (_server == null)
            {
                throw new ArgumentNullException("Server");
            }

            _server.Validate();
        }
    }
}