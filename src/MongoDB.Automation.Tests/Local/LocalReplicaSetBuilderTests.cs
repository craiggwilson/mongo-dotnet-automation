using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;

namespace MongoDB.Automation.Local
{
    [TestFixture]
    public class LocalReplicaSetBuilderTests
    {
        [Test]
        public void Build_should_set_replica_set_name_to_default_when_unspecified()
        {
            var subject = new LocalReplicaSetBuilder()
                .Port(27017, new LocalReplicaSetMongodBuilder().BinPath("funny"));

            var replSet = subject.Build();
            replSet.Name.Should().Be(Config.DefaultReplicaSetName);
        }

        [Test]
        public void Build_should_override_port_specified_in_template()
        {
            var subject = new LocalReplicaSetBuilder()
                .Port(30001, new LocalReplicaSetMongodBuilder().Port(30000).BinPath("funny"));

            var replSet = subject.Build();
            replSet.Members.Should().Contain(x => x.Port == 30001);
        }

        [Test]
        public void Build_should_create_a_replica_set_with_all_the_members()
        {
            var subject = new LocalReplicaSetBuilder()
                .Port(30000, 30001, 30002, new LocalReplicaSetMongodBuilder().BinPath("funny"));

            var replSet = subject.Build();
            replSet.Members.Count().Should().Be(3);
            replSet.Members.Should().Contain(x => x.Port == 30000);
            replSet.Members.Should().Contain(x => x.Port == 30001);
            replSet.Members.Should().Contain(x => x.Port == 30002);
        }
    }
}