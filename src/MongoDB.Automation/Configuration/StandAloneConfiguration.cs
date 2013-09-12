using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MongoDB.Automation.Configuration
{
    public class StandAloneConfiguration : IControllerConfiguration
    {
        private readonly IProcessConfiguration _server;

        public StandAloneConfiguration(IProcessConfiguration server)
        {
            _server = server;
        }

        public IProcessConfiguration Server
        {
            get { return _server; }
        }
    }
}