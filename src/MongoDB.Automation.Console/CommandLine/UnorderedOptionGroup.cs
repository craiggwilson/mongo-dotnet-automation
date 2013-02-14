using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoDB.Automation.Console.CommandLine
{
    public class UnorderedOptionGroup : IOption
    {
        private readonly List<INamedOption> _options;

        public UnorderedOptionGroup(IEnumerable<INamedOption> options)
        {
            if (options == null)
            {
                throw new ArgumentNullException("namedOptions");
            }

            _options = options.ToList();
        }

        public bool IsRequired
        {
            get { return _options.Any(x => x.IsRequired); }
        }

        public ParserContext Handle(ParserContext context)
        {
            var unprocessedTokens = new List<Token>();
            var processedOptions = new List<INamedOption>();

            INamedOption option;
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
                    unprocessedTokens.Add(context.LA(0));
                    context = context.Skip(1);
                }
            }

            var missingRequiredOptions = _options.Except(processedOptions).Where(x => x.IsRequired);
            if (missingRequiredOptions.Any())
            {
                throw new MissingCommandLineOptionException();
            }

            return new ParserContext(unprocessedTokens, context.Result);
        }

        private bool TryFindOption(string name, out INamedOption option)
        {
            option = _options.FirstOrDefault(x => x.Names.Contains(name));
            return option != null;
        }
    }
}