using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using MongoDB.Automation.Configuration;
using NSubstitute;
using NUnit.Framework;

namespace MongoDB.Automation
{
    [TestFixture]
    public class ReplicaSetControllerTests
    {
        [Test]
        public void Constructor_should_throw_if_configuration_is_null()
        {
            var processFactory = Substitute.For<IProcessFactory>();

            Action ctor = () => new ReplicaSetController(null, processFactory);

            ctor.ShouldThrow<ArgumentNullException>();
        }

        [Test]
        public void Constructor_should_throw_if_processFactory_is_null()
        {
            Action ctor = () => new ReplicaSetController(new ReplicaSetConfiguration(), null);

            ctor.ShouldThrow<ArgumentNullException>();
        }

        [Test]
        public void Members_should_contain_all_the_members()
        {
            var subject = CreateController();

            subject.Members.Should().HaveCount(3);
        }

        [Test]
        public void Primary_should_be_null_when_not_running()
        {
            var subject = CreateController();

            subject.Primary.Should().BeNull();
        }

        [Test]
        public void Secondaries_should_be_empty_when_not_running()
        {
            var subject = CreateController();

            subject.Secondaries.Should().HaveCount(0);
        }

        [Test]
        public void Arbiter_should_be_null_when_not_running()
        {
            var subject = CreateController();

            subject.Arbiter.Should().BeNull();
        }

        [Test]
        [KillMongoProcesses]
        public void WaitForFullAvailability_should_not_return_until_all_members_are_available()
        {
            var subject = CreateController();

            subject.Start(StartOptions.Clean);
            subject.WaitForFullAvailability(TimeSpan.FromMinutes(5));

            subject.Members.Should().HaveCount(3);
            subject.Primary.Should().NotBeNull();
            subject.Secondaries.Should().HaveCount(2);
            subject.Arbiter.Should().BeNull();
        }

        private ReplicaSetController CreateController()
        {
            var memberConfiguration = new LocalReplicaSetMongodConfigurationBuilder()
                .ExecutablePath(TestConfiguration.GetMongodPath())
                .DbPath("c:\\data\\db\\{replSet}\\{port}")
                .LogPath("c:\\data\\db\\{replSet}\\{port}.log")
                .NoHttpInterface()
                .NoJournal()
                .NoPrealloc()
                .OplogSize(10)
                .SmallFiles()
                .Build();

            var replicaSetConfiguration = new LocalReplicaSetConfigurationBuilder()
                .Port(30000, 30001, 30002, memberConfiguration)
                .Build();

            var processFactory = new DefaultProcessFactory();

            return new ReplicaSetController(replicaSetConfiguration, processFactory);
        }
    }
}