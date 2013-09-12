using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;

namespace MongoDB.Automation.Configuration
{
    [TestFixture]
    public class LocalReplicaSetConfigurationBuilderTests
    {
        [Test]
        public void Build_should_set_replica_set_name_to_default_when_unspecified()
        {
            var subject = new LocalReplicaSetConfigurationBuilder()
                .Port(27017, new LocalReplicaSetMongodConfigurationBuilder().ExecutablePath(TestConfiguration.GetMongodPath()).Build());

            var replSet = subject.Build();
            replSet.ReplicaSetName.Should().Be(Config.DefaultReplicaSetName);
        }

        [Test]
        public void Build_should_override_port_specified_in_template()
        {
            var subject = new LocalReplicaSetConfigurationBuilder()
                .Port(30001, new LocalReplicaSetMongodConfigurationBuilder().Port(30000).ExecutablePath(TestConfiguration.GetMongodPath()).Build());

            var replSet = subject.Build();
            replSet.Members.Should().Contain(x => ((LocalProcessConfiguration)x).Arguments.Single(f => f.Key == "port").Value == "30001");
        }

        [Test]
        public void Build_should_create_a_replica_set_with_all_the_members()
        {
            var subject = new LocalReplicaSetConfigurationBuilder()
                .Port(30000, 30001, 30002, new LocalReplicaSetMongodConfigurationBuilder().ExecutablePath(TestConfiguration.GetMongodPath()).Build());

            var replSet = subject.Build();
            replSet.Members.Count().Should().Be(3);
            replSet.Members.Should().Contain(x => ((LocalProcessConfiguration)x).Arguments.Single(f => f.Key == "port").Value == "30000");
            replSet.Members.Should().Contain(x => ((LocalProcessConfiguration)x).Arguments.Single(f => f.Key == "port").Value == "30001");
            replSet.Members.Should().Contain(x => ((LocalProcessConfiguration)x).Arguments.Single(f => f.Key == "port").Value == "30002");
        }
    }
}