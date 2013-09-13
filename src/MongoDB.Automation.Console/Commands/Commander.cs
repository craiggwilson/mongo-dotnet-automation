using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoDB.Automation.Console.Commands
{
    internal class Commander
    {
        private readonly List<ICommand> _commands;
        private string _defaultCommandName;

        public Commander()
        {
            _commands = new List<ICommand>();
        }

        public Commander AddDefaultCommand<TCommand>() where TCommand : ICommand, new()
        {
            return AddDefaultCommand(new TCommand());
        }

        public Commander AddDefaultCommand(ICommand command)
        {
            _defaultCommandName = command.Name;
            return AddCommand(command);
        }

        public Commander AddCommand<TCommand>() where TCommand : ICommand, new()
        {
            return AddCommand(new TCommand());
        }

        public Commander AddCommand(ICommand command)
        {
            _commands.Add(command);
            return this;
        }

        public void Execute(string[] args)
        {
            ICommand command = null;
            if (args.Length > 0)
            {
                command = _commands.SingleOrDefault(x => x.Name.Equals(args[0], StringComparison.InvariantCultureIgnoreCase));
                if (command != null)
                {
                    args = args.Skip(1).ToArray();
                }
            }

            if (command == null && _defaultCommandName != null)
            {
                command = _commands.Single(x => x.Name.Equals(_defaultCommandName, StringComparison.InvariantCultureIgnoreCase));
            }

            if (command == null)
            {
                throw new InvalidOperationException("Unable to determine a command.");
            }

            command.Execute(args);
        }
    }
}