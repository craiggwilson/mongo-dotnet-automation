using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;

namespace MongoDB.Automation.Console.CommandLine
{
    [TestFixture]
    public class ParserTests
    {
        [Test]
        public void TestUnorderedOptionGroup()
        {
            //var result = new Parser<OptionsClass>()
            //    .AddUnorderedOptions(list =>
            //    {
            //        list.Add(new NameValueOption<OptionsClass>("stringOpt", true, (n, v, r) => r.StringOpt = v));
            //        list.Add(new FlagOption<OptionsClass>("f", (n, r) => r.Flag = true));
            //        list.Add(new NameValueOption<OptionsClass>("intOpt", false, (n, v, r) => r.IntOpt = int.Parse(v)));
            //    })
            //    .Parse(new[] { "--stringOpt", "something", "-f", "--intOpt", "23" });


            //result.StringOpt.Should().Be("something");
            //result.IntOpt.Should().Be(23);
            //result.Flag.Should().BeTrue();
        }

        private class OptionsClass
        {
            public string StringOpt { get; set; }

            public int IntOpt { get; set; }

            public bool Flag { get; set; }
        }
    }
}