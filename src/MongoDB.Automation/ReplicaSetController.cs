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
    public class ReplicaSetController : IShardableInstanceProcessController
    {
        private BsonDocument _config;
        private readonly List<ReplicaSetMember> _members;
        private bool _isReplicaSetInitiated;
        private bool _hasArbiter;

        public ReplicaSetController(IEnumerable<IInstanceProcess<ReplicaSetMemberSettings>> processes)
        {
            _members = new List<ReplicaSetMember>();
            _isReplicaSetInitiated = false;
            Initialize(processes);
        }

        public string GetAddShardAddress()
        {
            return string.Format("{0}/{1}", _members[0].ReplicaSetName, _members[0].Address);
        }

        public void PrimaryIsAtAddress(MongoServerAddress address)
        {
            Util.Timeout(TimeSpan.FromMinutes(5),
                string.Format("Unable to make member at address {0} primary.", address),
                TimeSpan.FromSeconds(10),
                remaining => TryMakePrimaryAtAddress(address));
        }

        public void Start()
        {
            _members.ForEach(m => m.Start());

            if (!_isReplicaSetInitiated)
            {
                Console.WriteLine("Initiating replica set");
                _isReplicaSetInitiated = true;
                var replSetInitiate = new CommandDocument("replSetInitiate", _config);
                _members[0].RunReplicaSetInitiate(_config);
            }
        }

        public void Start(MongoServerAddress address)
        {
            var member = GetMember(address);
            member.Start();
            member.WaitForAvailability(TimeSpan.FromMinutes(10));
        }

        public void Stop()
        {
            Console.WriteLine("Stopping replica set");
            _members.ForEach(m => m.Stop());
        }

        public void Stop(MongoServerAddress address)
        {
            var member = GetMember(address);
            member.Stop();
        }

        public void WaitForAvailability(TimeSpan timeout)
        {
            // start any member that isn't up and running
            _members.ForEach(m => 
            {
                m.Start();
                m.WaitForAvailability(timeout);
            });

            var server = GetRunningMember().Connect();
            Util.Timeout(timeout, 
                string.Format("Unable to become fully available.", _members[0], timeout.TotalMilliseconds),
                TimeSpan.FromSeconds(5),
                remaining =>
                {
                    server.Connect(remaining);
                    return IsFullyAvailable(server);
                });
        }

        public void WaitForPrimary(TimeSpan timeout)
        {
            var server = GetRunningMember().Connect();
            Util.Timeout(timeout,
                string.Format("Unable to get a primary.", _members[0], timeout.TotalMilliseconds),
                TimeSpan.FromSeconds(5),
                remaining =>
                {
                    server.Connect(remaining);
                    return IsPrimaryAvailable(server);
                });
            Console.WriteLine("Primary is at address {0}", GetPrimaryMember().Address);
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
            return _members.SingleOrDefault(x => x.Type == ReplicaSetMemberType.Primary);
        }

        private ReplicaSetMember GetRunningMember()
        {
            return _members.FirstOrDefault(x => x.IsRunning);
        }

        private IEnumerable<ReplicaSetMember> GetSecondaryMembers()
        {
            return _members.Where(x => x.Type == ReplicaSetMemberType.Secondary);
        }

        private void IncrementConfigVersion()
        {
            var currentVersion = _config["version"].ToInt32();
            _config["version"] = currentVersion + 1;
        }

        private void Initialize(IEnumerable<IInstanceProcess<ReplicaSetMemberSettings>> processes)
        {
            var replicaSetName = processes.First().Settings.ReplicaSetName;
            _config = new BsonDocument("_id", replicaSetName);
            BsonArray memberConfigs = new BsonArray();
            _config.Add("members", memberConfigs);

            processes.ForEach((i, process) =>
            {
                if (process.Settings.ReplicaSetName != replicaSetName)
                {
                    throw new InvalidOperationException(
                        string.Format("There are at least two different replica set names specified: {0} and {1}",
                            replicaSetName,
                            process.Settings.ReplicaSetName));
                }

                var memberConfig = new BsonDocument
                {
                    { "_id", i },
                    { "priority", i == 0 ? 2 : 1 }, //make the first member the highest priority for a predictable primary
                    { "host", process.Address.ToString() }
                };

                if (process.Settings.IsArbiter)
                {
                    memberConfig["arbiterOnly"] = 1;
                    _hasArbiter = true;
                }

                memberConfigs.Add(memberConfig);

                _members.Add(new ReplicaSetMember
                {
                    ConfigEntry = memberConfig,
                    Process = process,
                    Type = ReplicaSetMemberType.Unknown
                });
            });
        }

        private bool IsFullyAvailable(MongoServer server)
        {
            int expectedPrimaries = 1;
            int expectedSecondaries = _hasArbiter ? _members.Count - 2 : _members.Count - 1;
            int expectedArbiters = _hasArbiter ? 1 : 0;

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
            UpdateMemberStatuses(GetRunningMember().Connect());
            var primaryMember = GetPrimaryMember();
            if (primaryMember.Address == address)
            {
                return true;
            }

            // to ensure a proper election, we need to be fully available...
            WaitForAvailability(TimeSpan.FromMinutes(5));

            Console.WriteLine("Attempting to force primary member to be at address {0}. Current primary is at address {1}.", address, primaryMember.Address);

            var targetMember = GetMember(address);

            _members.ForEach(m =>
            {
                m.ConfigEntry["priority"] = 1;
            });

            targetMember.ConfigEntry["priority"] = 2;

            IncrementConfigVersion();

            primaryMember.RunReplicaSetReconfig(_config);

            WaitForAvailability(TimeSpan.FromMinutes(5));

            if (targetMember.Type == ReplicaSetMemberType.Primary)
            {
                return true;
            }

            GetPrimaryMember().StepDown();
            WaitForAvailability(TimeSpan.FromMinutes(5));

            return targetMember.Type == ReplicaSetMemberType.Primary;
        }

        private void UpdateMemberStatuses(MongoServer server)
        {
            var result = server.GetDatabase("admin").RunCommand("replSetGetStatus");
            var memberStatuses = result.Response["members"].AsBsonArray;
            if (memberStatuses.Count != _members.Count)
            {
                throw new Exception(string.Format("Expected number of members was {0}, but replSetGetStatus reported {1}", _members.Count, memberStatuses.Count));
            }

            foreach (BsonDocument memberStatus in memberStatuses)
            {
                var id = memberStatus["_id"].ToInt32();
                var member = _members.SingleOrDefault(x => x.Id == id);
                if (member == null)
                {
                    throw new Exception(string.Format("A member with id {0} was reported, but it was not started by this instance manager.", id));
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
            public IInstanceProcess<ReplicaSetMemberSettings> Process;
            public ReplicaSetMemberType Type;

            public MongoServerAddress Address
            {
                get { return Process.Address; }
            }

            public string ReplicaSetName
            {
                get { return Process.Settings.ReplicaSetName; }
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

            public MongoServer Connect()
            {
                return Process.Connect();
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

            public void Start()
            {
                Process.Start();
            }

            public void StepDown()
            {
                try
                {
                    Console.WriteLine("Primary at address {0} is stepping down", Address);
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