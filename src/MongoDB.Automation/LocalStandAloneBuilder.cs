using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MongoDB.Automation
{
    public class LocalStandAloneBuilder : LocalMongodBuilder<LocalStandAloneBuilder, StandAloneSettings>
    {
        public LocalStandAloneBuilder(string binPath)
            : base(binPath)
        { }
    }
}