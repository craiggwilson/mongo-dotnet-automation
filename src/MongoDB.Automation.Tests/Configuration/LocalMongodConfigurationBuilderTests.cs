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
    public class LocalMongodConfigurationBuilderTests
    {
        [Test]
        public void Build_should_throw_an_exception_when_bin_path_is_not_set()
        {
            var builder = new LocalMongodConfigurationBuilder();

            Action build = () => builder.Build();
            build.ShouldThrow<AutomationException>();
        }
    }
}