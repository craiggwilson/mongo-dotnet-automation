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
    public class FlagOptionGroupTests
    {
        [Test]
        public void Should_process_uncombined_flags_and_leave_unknown_tokens_alone()
        {
            var tokens = new Token[] 
            {
                new NameToken("a"),
                new NameToken("c"),
                new ValueToken("cValue"),
                new NameToken("d"),
            };

            var result = new Dictionary<string, bool>();
            var context = new ParserContext(tokens, result);

            var subject = new FlagOptionGroup<Dictionary<string, bool>>(
                true,
                new[] 
                { 
                    new FlagOption<Dictionary<string, bool>>("a", (n, r) => r[n] = true),
                    new FlagOption<Dictionary<string, bool>>("b", (n, r) => r[n] = true),
                    new FlagOption<Dictionary<string, bool>>("d", (n, r) => r[n] = true),
                });

            context = subject.Handle(context);

            result.Should().Contain("a", true);
            result.Should().NotContainKey("b");
            result.Should().NotContainKey("c");
            result.Should().Contain("d", true);

            context.TokenCount.Should().Be(2);
            context.LA(0).Should().Match<NameToken>(x => x.Name == "c");
            context.LA(1).Should().Match<ValueToken>(x => x.Value == "cValue");
        }

        [Test]
        public void Should_process_combined_flags_and_leave_unknown_tokens_alone()
        {
            var tokens = new Token[] 
            {
                new NameToken("a"),
                new NameToken("c"),
                new ValueToken("cValue"),
                new NameToken("db"),
            };

            var result = new Dictionary<string, bool>();
            var context = new ParserContext(tokens, result);

            var subject = new FlagOptionGroup<Dictionary<string, bool>>(
                true,
                new[] 
                { 
                    new FlagOption<Dictionary<string, bool>>("a", (n, r) => r[n] = true),
                    new FlagOption<Dictionary<string, bool>>("b", (n, r) => r[n] = true),
                    new FlagOption<Dictionary<string, bool>>("d", (n, r) => r[n] = true),
                });

            context = subject.Handle(context);

            result.Should().Contain("a", true);
            result.Should().Contain("b", true);
            result.Should().NotContainKey("c");
            result.Should().Contain("d", true);

            context.TokenCount.Should().Be(2);
            context.LA(0).Should().Match<NameToken>(x => x.Name == "c");
            context.LA(1).Should().Match<ValueToken>(x => x.Value== "cValue");
        }

        [Test]
        public void Should_not_throw_exception_if_required_but_no_flags_match()
        {
            var tokens = new Token[] 
            {
                new NameToken("c"),
                new ValueToken("cValue"),
            };

            var result = new Dictionary<string, bool>();
            var context = new ParserContext(tokens, result);

            var subject = new FlagOptionGroup<Dictionary<string, bool>>(
                false,
                new[] 
                { 
                    new FlagOption<Dictionary<string, bool>>("a", (n, r) => r[n] = true),
                });

            context = subject.Handle(context);
            context.TokenCount.Should().Be(2);
        }

        [Test]
        public void Should_throw_exception_if_required_but_no_flags_match()
        {
            var tokens = new Token[] 
            {
                new NameToken("c"),
                new ValueToken("cValue"),
            };

            var result = new Dictionary<string, bool>();
            var context = new ParserContext(tokens, result);

            var subject = new FlagOptionGroup<Dictionary<string, bool>>(
                true,
                new[] 
                { 
                    new FlagOption<Dictionary<string, bool>>("a", (n, r) => r[n] = true),
                });

            Action a = () => subject.Handle(context);
            a.ShouldThrow<MissingCommandLineOptionException>();
        }
    }
}