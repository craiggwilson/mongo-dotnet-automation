using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MongoDB.Automation
{
    public class ShardRouterSettings : InstanceProcessSettingsBase
    {
        public ShardRouterSettings(int port)
            : base(port)
        { }
    }
}
