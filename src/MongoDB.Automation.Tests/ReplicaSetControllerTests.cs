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
    public class ReplicaSetControllerTests
    {
        [Test]
        public void Constructor_should_throw_if_replicaSetName_is_null_or_empty()
        {
            var process = Substitute.For<IInstanceProcess>();

            Action ctor = () => new ReplicaSetController("", new[] { process });

            ctor.ShouldThrow<ArgumentException>();
        }

        [Test]
        public void Constructor_should_throw_if_processes_is_null_or_empty()
        {
            var process = Substitute.For<IInstanceProcess>();

            Action ctor = () => new ReplicaSetController("rs0", Enumerable.Empty<IInstanceProcess>());

            ctor.ShouldThrow<ArgumentException>();
        }
    }
}