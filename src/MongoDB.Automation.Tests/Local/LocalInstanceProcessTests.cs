using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using FluentAssertions;
using MongoDB.Driver;
using System.IO;

namespace MongoDB.Automation.Local
{
    [TestFixture]
    public class LocalInstanceProcessTests
    {
        [Test]
        public void Constructor_should_throw_an_exception_when_executable_is_null_or_empty()
        {
            Action ctor = () => new LocalInstanceProcess("", new Dictionary<string, string>());
            ctor.ShouldThrow<ArgumentException>();
        }

        [Test]
        public void Constructor_hould_throw_when_mutual_dependency_detected()
        {
            var builder = new LocalMongodBuilder()
                .BinPath("something")
                .Set("fake1", "{fake2}")
                .Set("fake2", "{fake1}");

            Action build = () => builder.Build();
            build.ShouldThrow<AutomationException>();
        }

        [Test]
        public void Constructor_should_throw_when_cyclic_dependency_detected()
        {
            var builder = new LocalMongodBuilder()
                .BinPath("something")
                .Set("dep1", "{dep2}")
                .Set("dep2", "{dep3}")
                .Set("dep3", "{dep1}");

            Action build = () => builder.Build();
            build.ShouldThrow<AutomationException>();
        }

        [Test]
        public void Constructor_hould_throw_when_cyclic_dependency_detected_2()
        {
            var builder = new LocalMongodBuilder()
                .BinPath("something")
                .Set("dep1", "{dep2}")
                .Set("dep2", "{dep3}")
                .Set("dep3", "{dep1}")
                .Set("nodep", "yeah");

            Action build = () => builder.Build();
            build.ShouldThrow<AutomationException>();
        }

        [Test]
        public void Constructor_should_resolve_dependencies()
        {
            var builder = new LocalMongodBuilder()
                .BinPath("something")
                .Set("dep1", "{dep2}\\exists\\{port}") // port always exists
                .Set("dep2", "c:\\{nodep}")
                .Set("nodep", "yeah");

            var subject = builder.Build();
            var arguments = subject.Arguments;

            arguments.Should().Contain("--dep1 c:\\yeah\\exists\\27017");
            arguments.Should().Contain("--dep2 c:\\yeah");
            arguments.Should().Contain("--nodep yeah");
        }

        [Test]
        public void Should_not_be_running_after_construction()
        {
            var subject = new LocalInstanceProcess(@"blah", null);
            subject.IsRunning.Should().BeFalse();
        }

        [Test]
        public void Should_have_an_address_of_localhost_on_port_27017()
        {
            var subject = new LocalInstanceProcess(@"blah", null);
            subject.Address.ShouldBeEquivalentTo(new MongoServerAddress("localhost", 27017));
        }

        [Test]
        public void Should_create_data_directory_after_starting()
        {
            var subject = new LocalInstanceProcess(@"C:\MongoDB\mongodb-win32-x86_64-2.2.2\bin\mongod.exe", null);
            subject.Start(StartOptions.Clean);
            Directory.Exists(@"c:\data\db").Should().BeTrue();
            subject.Stop();
        }

        [Test]
        public void Should_be_able_to_connect_after_starting()
        {
            var sb = new StringBuilder();
            Config.SetOut(new StringWriter(sb));
            Config.SetError(new StringWriter(sb));
            var subject = new LocalInstanceProcess(@"C:\MongoDB\mongodb-win32-x86_64-2.2.2\bin\mongod.exe", null);
            subject.Start(StartOptions.Clean);
            var server = subject.Connect();
            subject.Stop();
        }
    }
}