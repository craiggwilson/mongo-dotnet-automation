using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MongoDB.Automation
{
    public class LocalStandAloneMongodBuilder : LocalMongodBuilder<LocalStandAloneMongodBuilder, StandAloneSettings>
    {
        public LocalStandAloneMongodBuilder(string binPath)
            : base(binPath)
        { }
    }
}