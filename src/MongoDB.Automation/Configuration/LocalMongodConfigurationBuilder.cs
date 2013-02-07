﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MongoDB.Automation.Configuration
{
    public class LocalMongodConfigurationBuilder : AbstractLocalMongodBuilder<LocalMongodConfigurationBuilder>
    {
        public LocalMongodConfigurationBuilder()
        { }

        public LocalMongodConfigurationBuilder(IEnumerable<KeyValuePair<string, string>> arguments)
            : base(arguments)
        { }

        public LocalMongodConfigurationBuilder(ILocalInstanceProcessConfiguration configuration)
            : base(configuration.Arguments)
        {
            BinPath(configuration.BinPath);
        }
    }
}