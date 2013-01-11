using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MongoDB.Automation
{
    public class ShardConfigServerSettings : InstanceProcessSettingsBase
    {
        public ShardConfigServerSettings(int port)
            : base(port)
        { }
    }
}
