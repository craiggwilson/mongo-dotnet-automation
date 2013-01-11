using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MongoDB.Automation
{
    public class StandAloneSettings : InstanceProcessSettingsBase
    {
        public StandAloneSettings(int port)
            : base(port)
        { }
    }
}