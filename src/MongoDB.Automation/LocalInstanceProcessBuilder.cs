using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MongoDB.Automation
{
    public abstract class LocalBuilder<TBuilder, TSettings> 
        where TBuilder : LocalBuilder<TBuilder, TSettings>
        where TSettings : IInstanceProcessSettings
    {
        private readonly Dictionary<string, Func<TSettings, string>> _argumentFactories;
        private readonly string _binPath;

        public LocalBuilder(string binPath)
        {
            _binPath = binPath;
            _argumentFactories = new Dictionary<string, Func<TSettings, string>>();
        }

        public TBuilder Set(string name, Func<TSettings, string> valueFactory)
        {
            _argumentFactories[name] = valueFactory;
            return (TBuilder)this;
        }

        public TBuilder Set(string name, string value)
        {
            return Set(name, i => value);
        }

        protected virtual void ApplySettings(TSettings settings)
        {
            Set("port", settings.Port.ToString());
        }

        protected string GetCommandArguments(TSettings settings)
        {
            ApplySettings(settings);
            List<string> args = new List<string>();
            foreach (var pair in _argumentFactories)
            {
                string arg = "--" + pair.Key;
                if (pair.Value != null)
                {
                    arg += " " + pair.Value(settings);
                }
                args.Add(arg);
            }

            return string.Join(" ", args.ToArray());
        }

        protected string GetExecutable(string name)
        {
            var path = Path.Combine(_binPath, name);
            if (Environment.OSVersion.Platform != PlatformID.MacOSX && Environment.OSVersion.Platform != PlatformID.Unix)
            {
                path += ".exe";
            }

            return path;
        }

        protected bool TryGetArgument(string name, TSettings settings, out string value)
        {
            Func<TSettings, string> valueFactory;
            if (_argumentFactories.TryGetValue(name, out valueFactory))
            {
                value = valueFactory(settings);
                return true;
            }

            value = null;
            return false;
        }
    }
}