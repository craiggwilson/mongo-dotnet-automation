using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using MongoDB.Automation.Configuration;

namespace MongoDB.Automation
{
    public class ReplicaSetController : IShardableController
    {
        private static readonly TimeSpan _defaultRetryTimeout = TimeSpan.FromMinutes(1);
        private static readonly TimeSpan _defaultRetryDelay = TimeSpan.FromSeconds(5);

        private readonly List<ReplicaSetMember> _members;
        private BsonDocument _config;
        private int? _arbiterPort;
        private string _replicaSetName;

        public ReplicaSetController(ReplicaSetConfiguration configuration, IProcessFactory processFactory)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException("configuration");
            }
            if (processFactory == null)
            {
                throw new ArgumentNullException("processFactory");
            }
            if (string.IsNullOrEmpty(configuration.ReplicaSetName))
            {
                throw new ArgumentException("Cannot be null or empty.", "replicaSetName");
            }
            if (configuration.Members == null || !configuration.Members.Any())
            {
                throw new ArgumentException("Cannot be null or empty.", "processes");
            }

            _replicaSetName = configuration.ReplicaSetName;
            _members = new List<ReplicaSetMember>();
            _arbiterPort = configuration.ArbiterPort;
            var processes = configuration.Members.Select(x => processFactory.Create(x));

            if(_arbiterPort.HasValue && !processes.Any(x => x.Address.Port == _arbiterPort.Value))
            {
                throw new ArgumentException("When an arbiter port is specified, it must exist in the members.", "configuration.ArbiterPort");
            }

            Initialize(processes);
        }

        public MongoServerAddress Arbiter
        {
            get 
            {
                var arbiter = GetArbiter();
                if (arbiter == null)
                {
                    return null;
                }

                return arbiter.Address;
            }
        }

        public bool HasArbiter
        {
            get { return _arbiterPort.HasValue; }
        }

        public IEnumerable<MongoServerAddress> Members
        {
            get { return _members.Select(x => x.Address); }
        }

        public string Name
        {
            get { return _replicaSetName; }
        }

        public MongoServerAddress Primary
        {
            get
            {
                var primary = GetPrimaryMember();
                if (primary == null)
                {
                    return null;
                }

                return primary.Address;
            }
        }

        public IEnumerable<MongoServerAddress> Secondaries
        {
            get { return GetSecondaryMembers().Select(x => x.Address); }
        }

        public string GetAddShardAddress()
        {
            return string.Format("{0}/{1}", _replicaSetName, _members[0].Address);
        }

        public IControllerConfiguration GetConfiguration()
        {
            return new ReplicaSetConfiguration
            {
                ReplicaSetName = _replicaSetName,
                Members = _members.Select(x => x.Process.GetConfiguration()).OfType<IProcessConfiguration>(),
                ArbiterPort = _arbiterPort
            };
        }

        public void MakePrimary(MongoServerAddress address)
        {
            Config.Out.WriteLine("Forcing primary to be at address {0}.", address);
            Retry.WithTimeout(
                _ => TryMakePrimaryAtAddress(address),
                TimeSpan.FromMinutes(5),
                TimeSpan.FromSeconds(10));
            Config.Out.WriteLine("Primary is at address {0}.", address);
        }

        public void Start(StartOptions options)
        {
            Config.Out.WriteLine("Starting replica set.");
            _members.ForEach(m => m.Start(options));

            if (options == StartOptions.Clean)
            {
                Config.Out.WriteLine("Initiating replica set.");
                var replSetInitiate = new CommandDocument("replSetInitiate", _config);
                _members[0].RunReplicaSetInitiate(_config);
                Config.Out.WriteLine("Replica set initiated.");
            }
            Config.Out.WriteLine("Replica set started.");
        }

        public void Start(MongoServerAddress address, StartOptions options)
        {
            var member = GetMember(address);
            member.Start(options);
            member.WaitForAvailability(TimeSpan.FromMinutes(10));
        }

        public void Stop()
        {
            Config.Out.WriteLine("Stopping replica set.", _replicaSetName);
            _members.ForEach(m => m.Stop());
            Config.Out.WriteLine("Replica set stopped.", _replicaSetName);
        }

        public void Stop(MongoServerAddress address)
        {
            var member = GetMember(address);
            member.Stop();
        }

        public void WaitForFullAvailability(TimeSpan timeout)
        {
            Config.Out.WriteLine("Waiting for replica set to become fully available.");
            // start any member that isn't up and running
            _members.ForEach(m => 
            {
                m.Start(StartOptions.None);
                m.WaitForAvailability(timeout);
            });

            Retry.WithTimeout(
                remaining =>
                {
                    var member = GetRunningMember();
                    if (member == null)
                    {
                        return false;
                    }
                    var server = member.Connect(remaining);
                    return IsFullyAvailable(server);
                },
                timeout, 
                _defaultRetryDelay);
            Config.Out.WriteLine("Replica set is fully available.");
        }

        public void WaitForPrimary(TimeSpan timeout)
        {
            Config.Out.WriteLine("Waiting for primary.");
            Retry.WithTimeout(
                remaining =>
                {
                    var member = GetRunningMember();
                    if (member == null)
                    {
                        return false;
                    }
                    var server = member.Connect(remaining);
                    return IsPrimaryAvailable(server);
                },
                timeout,
                _defaultRetryDelay);

            Config.Out.WriteLine("Primary is at address {0}.", GetPrimaryMember().Address);
        }

        private ReplicaSetMember GetArbiter()
        {
            var running = GetRunningMember();
            if (running == null)
            {
                return null;
            }
            UpdateMemberStatuses(running.Connect(_defaultRetryTimeout));
            return _members.SingleOrDefault(x => x.Type == ReplicaSetMemberType.Arbiter);
        }

        private ReplicaSetMember GetMember(MongoServerAddress address)
        {
            var member = _members.SingleOrDefault(x => x.Address == address);
            if (member == null)
            {
                throw new Exception(string.Format("No member was setup with address {0}", address));
            }

            return member;
        }

        private ReplicaSetMember GetPrimaryMember()
        {
            var running = GetRunningMember();
            if (running == null)
            {
                return null;
            }

            UpdateMemberStatuses(running.Connect(_defaultRetryTimeout));
            return _members.SingleOrDefault(x => x.Type == ReplicaSetMemberType.Primary);
        }

        private ReplicaSetMember GetRunningMember()
        {
            return _members.FirstOrDefault(x => x.IsRunning);
        }

        private IEnumerable<ReplicaSetMember> GetSecondaryMembers()
        {
            var running = GetRunningMember();
            if (running == null)
            {
                return Enumerable.Empty<ReplicaSetMember>();
            }
            UpdateMemberStatuses(running.Connect(_defaultRetryTimeout));
            return _members.Where(x => x.Type == ReplicaSetMemberType.Secondary);
        }

        private void IncrementConfigVersion()
        {
            var currentVersion = _config["version"].ToInt32();
            _config["version"] = currentVersion + 1;
        }

        private void Initialize(IEnumerable<IProcess> processes)
        {
            _config = new BsonDocument("_id", _replicaSetName);
            BsonArray memberConfigs = new BsonArray();
            _config.Add("members", memberConfigs);

            processes.ForEach((i, process) =>
            {
                var memberConfig = new BsonDocument
                {
                    { "_id", i },
                    { "priority", i == 0 ? 2 : 1 }, //make the first member the highest priority for a predictable primary
                    { "host", process.Address.ToString() }
                };

                if (_arbiterPort.HasValue && process.Address.Port == _arbiterPort.Value)
                {
                    memberConfig["arbiterOnly"] = 1;
                }

                memberConfigs.Add(memberConfig);

                _members.Add(new ReplicaSetMember
                {
                    ConfigEntry = memberConfig,
                    Process = process,
                    Type = ReplicaSetMemberType.Unknown
                });
            });

            Config.Out.WriteLine("Replica set {0} config: {1}", _replicaSetName, _config.ToJson());
        }

        private bool IsFullyAvailable(MongoServer server)
        {
            int expectedPrimaries = 1;
            int expectedSecondaries = HasArbiter ? _members.Count - 2 : _members.Count - 1;
            int expectedArbiters = HasArbiter ? 1 : 0;

            return MatchesAvailability(server, expectedPrimaries, expectedSecondaries, expectedArbiters);
        }

        private bool IsPrimaryAvailable(MongoServer server)
        {
            return MatchesAvailability(server, 1, 0, 0);
        }

        private bool MatchesAvailability(MongoServer server, int expectedPrimaries, int expectedSecondaries, int expectedArbiters)
        {
            int numPrimaries = 0;
            int numSecondaries = 0;
            int numArbiters = 0;

            UpdateMemberStatuses(server);

            foreach (var member in _members)
            {
                switch (member.Type)
                {
                    case ReplicaSetMemberType.Primary:
                        numPrimaries++;
                        break;
                    case ReplicaSetMemberType.Secondary:
                        numSecondaries++;
                        break;
                    case ReplicaSetMemberType.Arbiter:
                        numArbiters++;
                        break;
                }
            }

            return numPrimaries >= expectedPrimaries
                && numSecondaries >= expectedSecondaries
                && numArbiters >= expectedArbiters;
        }

        private bool TryMakePrimaryAtAddress(MongoServerAddress address)
        {
            UpdateMemberStatuses(GetRunningMember().Connect(_defaultRetryTimeout));
            var primaryMember = GetPrimaryMember();
            if (primaryMember.Address == address)
            {
                return true;
            }

            // to ensure a proper election, we need to be fully available...
            WaitForFullAvailability(TimeSpan.FromMinutes(5));

            Config.Out.WriteLine("Attempting to force primary member to be at address {0}. Current primary is at address {1}.", address, primaryMember.Address);

            var targetMember = GetMember(address);

            _members.ForEach(m =>
            {
                m.ConfigEntry["priority"] = 1;
            });

            targetMember.ConfigEntry["priority"] = 2;

            IncrementConfigVersion();

            Config.Out.WriteLine("Reconfiguring replica set.", _replicaSetName);
            primaryMember.RunReplicaSetReconfig(_config);

            WaitForFullAvailability(TimeSpan.FromMinutes(5));

            if (targetMember.Type == ReplicaSetMemberType.Primary)
            {
                return true;
            }

            Config.Out.WriteLine("Changing priority did not create the member at address {0} primary.");
            GetPrimaryMember().StepDown();
            WaitForFullAvailability(TimeSpan.FromMinutes(5));

            return targetMember.Type == ReplicaSetMemberType.Primary;
        }

        private void UpdateMemberStatuses(MongoServer server)
        {
            // TODO: maybe should be using db.isMaster()...
            var result = server.GetDatabase("admin").RunCommand("replSetGetStatus");
            var memberStatuses = result.Response["members"].AsBsonArray;
            if (memberStatuses.Count != _members.Count)
            {
                var message = string.Format("Expected number of members was {0}, but replSetGetStatus reported {1}", _members.Count, memberStatuses.Count);
                Config.Error.WriteLine(message);
                throw new AutomationException(message);
            }

            foreach (BsonDocument memberStatus in memberStatuses)
            {
                var id = memberStatus["_id"].ToInt32();
                var member = _members.SingleOrDefault(x => x.Id == id);
                if (member == null)
                {
                    var message = string.Format("A member with id {0} was reported, but it was not started by this instance manager.", id);
                    Config.Error.WriteLine(message);
                    throw new AutomationException(message);
                }

                member.UpdateStatus(memberStatus);
            }
        }

        private enum ReplicaSetMemberType
        {
            Unknown,
            Primary,
            Secondary,
            Arbiter
        }

        private class ReplicaSetMember
        {
            public BsonDocument ConfigEntry;
            public IProcess Process;
            public ReplicaSetMemberType Type;

            public MongoServerAddress Address
            {
                get { return Process.Address; }
            }

            public int Id
            {
                get { return ConfigEntry["_id"].AsInt32; }
            }

            public bool IsArbiter
            {
                get { return ConfigEntry.GetValue("arbiterOnly", BsonBoolean.False).AsBoolean; }
            }

            public bool IsRunning
            {
                get { return Process.IsRunning; }
            }

            public MongoServer Connect(TimeSpan timeout)
            {
                return Process.Connect(timeout);
            }

            public CommandResult RunReplicaSetGetStatus()
            {
                return Process.RunAdminCommand("replSetGetStatus");
            }

            public CommandResult RunReplicaSetInitiate(BsonDocument config)
            {
                return Process.RunAdminCommand(new CommandDocument 
                {
                    { "replSetInitiate", config }
                });
            }

            public CommandResult RunReplicaSetReconfig(BsonDocument config)
            {
                return Process.RunAdminCommand(new CommandDocument 
                {
                    { "replSetReconfig", config }
                });
            }

            public void Start(StartOptions options)
            {
                Process.Start(options);
            }

            public void StepDown()
            {
                try
                {
                    Config.Out.WriteLine("Primary at address {0} is stepping down.", Address);
                    Process.RunAdminCommand(new CommandDocument { { "replSetStepDown", TimeSpan.FromMinutes(5).TotalSeconds }, { "force", true } });
                }
                catch (EndOfStreamException)
                { } // this is expected
            }

            public void Stop()
            {
                Process.Stop();
                Type = ReplicaSetMemberType.Unknown;
            }

            public void WaitForAvailability(TimeSpan timeout)
            {
                Process.WaitForAvailability(timeout);
            }

            public void UpdateStatus(BsonDocument memberStatus)
            {
                switch (memberStatus["state"].ToInt32())
                {
                    case 1:
                        Type = ReplicaSetMemberType.Primary;
                        break;
                    case 2:
                        Type = ReplicaSetMemberType.Secondary;
                        break;
                    case 7:
                        Type = ReplicaSetMemberType.Arbiter;
                        break;
                    default:
                        Type = ReplicaSetMemberType.Unknown;
                        break;
                }
            }
        }
    }
}