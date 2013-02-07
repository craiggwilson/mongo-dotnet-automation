using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;

namespace MongoDB.Automation
{
    [TestFixture]
    public class AbstractProcessTests
    {
        [Test]
        public void Should_throw_AutomationException_when_connecting_to_an_instance_that_is_not_running()
        {
            var subject = Substitute.For<AbstractProcess>();
            subject.IsRunning.Returns(false);

            Action connect = () => subject.Connect();
            connect.ShouldThrow<AutomationException>();
        }
    }
}