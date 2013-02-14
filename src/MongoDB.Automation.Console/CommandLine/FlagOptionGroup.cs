using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoDB.Automation.Console.CommandLine
{
    public class FlagOptionGroup<T> : IOption
    {
        private readonly List<FlagOption<T>> _options;
        private readonly bool _required;

        public FlagOptionGroup(bool required, IEnumerable<FlagOption<T>> options)
        {
            _required = required;
            _options = options.ToList();
        }

        public bool IsRequired
        {
            get { return _required; }
        }

        public ParserContext Handle(ParserContext context)
        {
            var unprocessedTokens = new List<Token>();
            var processedOptions = new List<INamedOption>();

            FlagOption<T> option;
            while (context.TokenCount > 0)
            {
                var nameToken = context.LA(0) as NameToken;
                if (nameToken != null && TryFindOption(nameToken.Name, out option))
                {
                    context = option.Handle(context);
                    processedOptions.Add(option);
                }
                else
                {
                    if (nameToken != null)
                    { 
                        // let's try and break this guy up into short options...
                        var flatTokens = nameToken.Name.ToCharArray().Select(x => x.ToString()).ToList();
                        var matchedOptions = _options.Where(x => x.Names.Any(n => flatTokens.Contains(n))).ToList();
                        if (matchedOptions.Count == flatTokens.Count)
                        {
                            foreach (var matchedOption in matchedOptions)
                            {
                                var tempContext = new ParserContext(new [] { new NameToken(matchedOption.Names.First()) }, context.Result);
                                matchedOption.Handle(tempContext);
                                processedOptions.Add(matchedOption);
                            }

                            context = context.Skip(1);
                            continue;
                        }
                    }

                    unprocessedTokens.Add(context.LA(0));
                    context = context.Skip(1);
                }
            }

            if (_required && !processedOptions.Any())
            {
                throw new MissingCommandLineOptionException();
            }

            return new ParserContext(unprocessedTokens, context.Result);
        }

        private bool TryFindOption(string name, out FlagOption<T> option)
        {
            option = _options.FirstOrDefault(x => x.Names.Contains(name));
            return option != null;
        }
    }
}