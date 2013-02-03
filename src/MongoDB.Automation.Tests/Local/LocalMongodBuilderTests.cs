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
    public class LocalMongodBuilderTests
    {
        [Test]
        public void Build_should_throw_an_exception_when_bin_path_is_not_set()
        {
            var builder = new LocalMongodBuilder();

            Action build = () => builder.Build();
            build.ShouldThrow<AutomationException>();
        }
    }
}