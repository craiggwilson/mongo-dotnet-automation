using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace MongoDB.Automation
{
    public class ShardController : IInstanceProcessController
    {
        private readonly List<Shard> _shards;
        private readonly List<IInstanceProcess<ShardConfigServerSettings>> _configServers;
        private readonly List<IInstanceProcess<ShardRouterSettings>> _routers;
        private bool _isInitiated;

        public ShardController(IEnumerable<IShardableInstanceProcessController> shards, IEnumerable<IInstanceProcess<ShardConfigServerSettings>> configServers, IEnumerable<IInstanceProcess<ShardRouterSettings>> routers)
        {
            _shards = shards.Select((x, i) => new Shard(string.Format("shard_{0}", i), x)).ToList();
            _configServers = configServers.ToList();
            _routers = routers.ToList();
            _isInitiated = false;
        }

        public void Start()
        {
            _shards.ForEach(s => s.Start());
            _configServers.ForEach(cs => cs.Start());
            _routers.ForEach(r => r.Start());

            if (!_isInitiated)
            {
                var router = _routers.First();
                _shards.ForEach(s => s.AddToCluster(router));
                _isInitiated = true;
            }
        }

        public void Stop()
        {
            _shards.ForEach(s => s.Stop());
            _configServers.ForEach(cs => cs.Stop());
            _routers.ForEach(r => r.Stop());
        }

        public void WaitForAvailability(TimeSpan timeout)
        {
            _shards.ForEach(s => s.WaitForAvailability(timeout));
            _configServers.ForEach(cs => cs.WaitForAvailability(timeout));
            _routers.ForEach(r => r.WaitForAvailability(timeout));
        }

        private class Shard
        {
            private readonly IShardableInstanceProcessController _controller;
            private readonly string _name;

            public Shard(string name, IShardableInstanceProcessController controller)
            {
                _name = name;
                _controller = controller;
            }

            public string Name
            {
                get { return _name; }
            }

            public void AddToCluster(IInstanceProcess<ShardRouterSettings> router)
            {
                var cmd = new CommandDocument();
                cmd.Add("addShard", _controller.GetAddShardAddress());
                cmd.Add("name", _name);
                router.RunAdminCommand(cmd);
            }

            public void Start()
            {
                _controller.Start();
            }

            public void Stop()
            {
                _controller.Stop();
            }

            public void WaitForAvailability(TimeSpan timeout)
            {
                _controller.WaitForAvailability(timeout);
            }
        }
    }
}