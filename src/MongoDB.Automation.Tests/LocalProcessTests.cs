using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using FluentAssertions;
using MongoDB.Automation.Configuration;
using MongoDB.Driver;
using System.IO;

namespace MongoDB.Automation
{
    [TestFixture]
    public class LocalProcessTests
    {
        [Test]
        public void Constructor_should_throw_an_exception_when_executable_is_null_or_empty()
        {
            var dict = new Dictionary<string, string>();
            Action ctor = () => new LocalProcess("", dict);
            ctor.ShouldThrow<ArgumentException>();
        }

        [Test]
        public void Constructor_should_throw_when_mutual_dependency_detected()
        {
            var config = new LocalMongodConfigurationBuilder()
                .ExecutablePath("something")
                .Set("fake1", "{fake2}")
                .Set("fake2", "{fake1}")
                .Build();

            Action build = () => new LocalProcess(config.ExecutablePath, config.Arguments);
            build.ShouldThrow<AutomationException>();
        }

        [Test]
        public void Constructor_should_throw_when_cyclic_dependency_detected()
        {
            var config = new LocalMongodConfigurationBuilder()
                .ExecutablePath("something")
                .Set("dep1", "{dep2}")
                .Set("dep2", "{dep3}")
                .Set("dep3", "{dep1}")
                .Build();

            Action build = () => new LocalProcess(config.ExecutablePath, config.Arguments);
            build.ShouldThrow<AutomationException>();
        }

        [Test]
        public void Constructor_should_throw_when_cyclic_dependency_detected_2()
        {
            var config = new LocalMongodConfigurationBuilder()
                .ExecutablePath("something")
                .Set("dep1", "{dep2}")
                .Set("dep2", "{dep3}")
                .Set("dep3", "{dep1}")
                .Set("nodep", "yeah")
                .Build();

            Action build = () => new LocalProcess(config.ExecutablePath, config.Arguments);
            build.ShouldThrow<AutomationException>();
        }

        [Test]
        public void Constructor_should_resolve_dependencies()
        {
            var config = new LocalMongodConfigurationBuilder()
                .ExecutablePath(TestConfiguration.GetMongodPath())
                .Set("dep1", "{dep2}\\exists\\{port}") // port always exists
                .Set("dep2", "c:\\{nodep}")
                .Set("nodep", "yeah")
                .Build();

            var subject = new LocalProcess(config.ExecutablePath, config.Arguments);
            var arguments = subject.Arguments;

            arguments.Should().Contain("--dep1 c:\\yeah\\exists\\27017");
            arguments.Should().Contain("--dep2 c:\\yeah");
            arguments.Should().Contain("--nodep yeah");
        }

        [Test]
        public void Running_should_be_false_after_construction()
        {
            var config = new LocalProcessConfiguration(TestConfiguration.GetMongodPath(), null);
            var subject = new LocalProcess(config.ExecutablePath, config.Arguments);
            subject.IsRunning.Should().BeFalse();
        }

        [Test]
        public void Address_should_have_a_default_of_localhost_on_port_27017()
        {
            var config = new LocalProcessConfiguration(TestConfiguration.GetMongodPath(), null);
            var subject = new LocalProcess(config.ExecutablePath, config.Arguments);
            subject.Address.ShouldBeEquivalentTo(new MongoServerAddress("localhost", 27017));
        }

        [Test]
        [KillMongoProcesses]
        public void Start_should_create_data_directory()
        {
            var config = new LocalProcessConfiguration(TestConfiguration.GetMongodPath(), null);
            var subject = new LocalProcess(config.ExecutablePath, config.Arguments);
            subject.Start(StartOptions.Clean);
            Directory.Exists(@"c:\data\db").Should().BeTrue();
        }

        [Test]
        [KillMongoProcesses]
        public void Connect_should_be_successful_when_IsRunning_is_true()
        {
            var config = new LocalProcessConfiguration(TestConfiguration.GetMongodPath(), null);
            var subject = new LocalProcess(config.ExecutablePath, config.Arguments);
            subject.Start(StartOptions.Clean);
            var server = subject.Connect();
        }
    }
}