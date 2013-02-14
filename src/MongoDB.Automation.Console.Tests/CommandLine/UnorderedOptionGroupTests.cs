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
    public class UnorderedOptionGroupTests
    {
        [Test]
        public void Should_handle_options_when_found_in_order()
        {
            var tokens = new Token[] 
            {
                new NameToken("s"),
                new ValueToken("sValue"),
                new NameToken("i"),
                new ValueToken("iValue")
            };

            var result = new Dictionary<string, string>();
            var context = new ParserContext(tokens, result);
            var sOption = new NameValueOption<Dictionary<string, string>>("s", true, (n, v, r) => r["s"] = v);
            var iOption = new NameValueOption<Dictionary<string, string>>("i", true, (n, v, r) => r["i"] = v);
            var subject = new UnorderedOptionGroup(new[] { sOption, iOption });

            context = subject.Handle(context);

            result.Should().Contain("s", "sValue");
            result.Should().Contain("i", "iValue");
        }

        [Test]
        public void Should_handle_options_when_found_out_of_order()
        {
            var tokens = new Token[] 
            {
                new NameToken("i"),
                new ValueToken("iValue"),
                new NameToken("s"),
                new ValueToken("sValue")
            };

            var result = new Dictionary<string, string>();
            var context = new ParserContext(tokens, result);
            var sOption = new NameValueOption<Dictionary<string, string>>("s", true, (n, v, r) => r["s"] = v);
            var iOption = new NameValueOption<Dictionary<string, string>>("i", true, (n, v, r) => r["i"] = v);
            var subject = new UnorderedOptionGroup(new[] { sOption, iOption });

            context = subject.Handle(context);

            result.Should().Contain("s", "sValue");
            result.Should().Contain("i", "iValue");
        }

        [Test]
        public void Should_return_context_with_unprocessed_tokens()
        {
            var tokens = new Token[] 
            {
                new NameToken("i"),
                new ValueToken("iValue"),
                new NameToken("u"),
                new ValueToken("uValue"),
                new NameToken("s"),
                new ValueToken("sValue"),
                new NameToken("f"),
                new ValueToken("f1"),
                new ValueToken("f2")
            };

            var result = new Dictionary<string, string>();
            var context = new ParserContext(tokens, result);
            var sOption = new NameValueOption<Dictionary<string, string>>("s", true, (n, v, r) => r["s"] = v);
            var iOption = new NameValueOption<Dictionary<string, string>>("i", true, (n, v, r) => r["i"] = v);
            var subject = new UnorderedOptionGroup(new[] { sOption, iOption });

            context = subject.Handle(context);

            result.Should().Contain("s", "sValue");
            result.Should().Contain("i", "iValue");

            var remaining = context.ReadAll().ToList();
            remaining.Count.Should().Be(5);
            remaining[0].Should().Match<NameToken>(x => x.Name == "u");
            remaining[1].Should().Match<ValueToken>(x => x.Value == "uValue");
            remaining[2].Should().Match<NameToken>(x => x.Name == "f");
            remaining[3].Should().Match<ValueToken>(x => x.Value == "f1");
            remaining[4].Should().Match<ValueToken>(x => x.Value == "f2");
        }

        [Test]
        public void Should_throw_exception_when_missing_required_option()
        {
            var tokens = new Token[] 
            {
                new NameToken("i"),
                new ValueToken("iValue")
            };

            var result = new Dictionary<string, string>();
            var context = new ParserContext(tokens, result);
            var sOption = new NameValueOption<Dictionary<string, string>>("s", true, (n, v, r) => r["s"] = v);
            var iOption = new NameValueOption<Dictionary<string, string>>("i", true, (n, v, r) => r["i"] = v);
            var subject = new UnorderedOptionGroup(new[] { sOption, iOption });

            Action a = () => subject.Handle(context);
            a.ShouldThrow<MissingCommandLineOptionException>();
        }
    }
}