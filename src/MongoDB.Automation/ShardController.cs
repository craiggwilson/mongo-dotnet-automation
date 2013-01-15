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
        private readonly List<IInstanceProcess> _configServers;
        private readonly List<IInstanceProcess> _routers;
        private bool _isInitiated;

        public ShardController(IEnumerable<IShardableInstanceProcessController> shards, IEnumerable<IInstanceProcess> configServers, IEnumerable<IInstanceProcess> routers)
        {
            _shards = shards.Select((x, i) => new Shard(string.Format("shard_{0}", i), x)).ToList();
            _configServers = configServers.ToList();
            _routers = routers.ToList();
            _isInitiated = false;
        }

        public void Start(StartOptions options)
        {
            Config.Out.WriteLine("Starting shards.");
            _shards.ForEach(s => s.Start(options));
            Config.Out.WriteLine("Starting config servers.");
            _configServers.ForEach(cs => cs.Start(options));
            Config.Out.WriteLine("Starting routers.");
            _routers.ForEach(r => r.Start(options));

            if (!_isInitiated)
            {
                var router = _routers.First();
                Config.Out.WriteLine("Adding shards to the cluster.");
                _shards.ForEach(s => s.AddToCluster(router));
                _isInitiated = true;
            }
        }

        public void Stop()
        {
            Config.Out.WriteLine("Stopping shards.");
            _shards.ForEach(s => s.Stop());
            Config.Out.WriteLine("Stopping config servers.");
            _configServers.ForEach(cs => cs.Stop());
            Config.Out.WriteLine("Stopping routers.");
            _routers.ForEach(r => r.Stop());
        }

        public void WaitForFullAvailability(TimeSpan timeout)
        {
            Config.Out.WriteLine("Waiting for full availability.");
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

            public void AddToCluster(IInstanceProcess router)
            {
                var address = _controller.GetAddShardAddress();
                Config.Out.WriteLine("Adding shard to cluster: {0}.", address);
                var cmd = new CommandDocument();
                cmd.Add("addShard", address);
                cmd.Add("name", _name);
                router.RunAdminCommand(cmd);
            }

            public void Start(StartOptions options)
            {
                _controller.Start(options);
            }

            public void Stop()
            {
                _controller.Stop();
            }

            public void WaitForAvailability(TimeSpan timeout)
            {
                _controller.WaitForFullAvailability(timeout);
            }
        }
    }
}