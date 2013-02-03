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
        public void Constructor_should_throw_an_exception_when_executable_does_not_exist()
        {
            Action ctor = () => new LocalInstanceProcess(@"C:\never_gonna_be_there\mongod.exe", new Dictionary<string, string>());
            ctor.ShouldThrow<ArgumentException>();
        }

        [Test]
        public void Should_not_be_running_after_construction()
        {
            var subject = new LocalInstanceProcess(@"C:\MongoDB\mongodb-win32-x86_64-2.2.2\bin\mongod.exe", null);
            subject.IsRunning.Should().BeFalse();
        }

        [Test]
        public void Should_have_an_address_of_localhost_on_port_27017()
        {
            var subject = new LocalInstanceProcess(@"C:\MongoDB\mongodb-win32-x86_64-2.2.2\bin\mongod.exe", null);
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
            var subject = new LocalInstanceProcess(@"C:\MongoDB\mongodb-win32-x86_64-2.2.2\bin\mongod.exe", null);
            subject.Start(StartOptions.Clean);
            var server = subject.Connect();
            subject.Stop();
        }
    }
}