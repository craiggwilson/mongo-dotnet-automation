using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MongoDB.Automation
{
    public class LocalMongosBuilder : LocalBuilder<LocalMongosBuilder, ShardRouterSettings>, IInstanceProcessFactory<ShardRouterSettings>
    {
        private readonly List<MongoServerAddress> _configServers;

        public LocalMongosBuilder(string binPath)
            : base(binPath)
        {
            _configServers = new List<MongoServerAddress>();
        }

        public IInstanceProcess<ShardRouterSettings> Create(ShardRouterSettings settings)
        {
            Set("port", settings.Port.ToString());
            return new LocalInstanceProcess<ShardRouterSettings>(
                GetExecutable("mongos"),
                GetCommandArguments(settings),
                settings);
        }

        public LocalMongosBuilder ConfigServer(MongoServerAddress address)
        {
            _configServers.Add(address);
            return Set("configdb", string.Join(" ", _configServers.Select(x => x.ToString()).ToArray()));
        }
    }
}