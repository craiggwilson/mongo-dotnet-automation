using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace MongoDB.Automation.Configuration
{
    public abstract class AbstractLocalConfigurationBuilder<T>
        where T : AbstractLocalConfigurationBuilder<T>
    {
        private readonly Dictionary<string, string> _arguments;
        private string _executablePath;

        protected AbstractLocalConfigurationBuilder()
        {
            _arguments = new Dictionary<string, string>();
        }

        protected AbstractLocalConfigurationBuilder(IEnumerable<KeyValuePair<string,string>> arguments)
        {
            if (arguments == null)
            {
                throw new ArgumentNullException("arguments");
            }

            _arguments = arguments.ToDictionary(x => x.Key, x => x.Value);
        }

        public T BindIP(IPAddress ip)
        {
            return Set(Constants.BIND_IP, ip.ToString());
        }

        public LocalProcessConfiguration Build()
        {
            if (_executablePath == null)
            {
                throw new AutomationException("Must provide an executable path.");
            }

            return new LocalProcessConfiguration
            {
                ExecutablePath = _executablePath,
                Arguments = _arguments
            };
        }

        public T Config(string configPath)
        {
            return Set(Constants.CONFIG, configPath);
        }

        public T ExecutablePath(string binPath)
        {
            _executablePath = binPath;
            return (T)this;
        }

        public T Ipv6()
        {
            return Set(Constants.IPV6);
        }

        public T KeyFile(string keyFilePath)
        {
            return Set(Constants.KEY_FILE, keyFilePath);
        }

        public T LogAppend()
        {
            return Set(Constants.LOG_APPEND);
        }

        public T LogPath(string logPath)
        {
            return Set(Constants.LOG_PATH, logPath);
        }

        public T MaxConnections(int maxConnections)
        {
            return Set(Constants.MAX_CONN, maxConnections.ToString());
        }

        public T NoHttpInterface()
        {
            return Set(Constants.NO_HTTP_INTERFACE);
        }

        public T Port(int port)
        {
            return Set(Constants.PORT, port.ToString());
        }

        public T Set(string name)
        {
            return Set(name, null);
        }

        public T Set(string name, string value)
        {
            _arguments[name] = value;
            return (T)this;
        }

        public T Verbosity(int count)
        {
            if (count < 0)
            {
                return (T)this;
            }

            return Set(new String(Constants.V, count));
        }
    }
}