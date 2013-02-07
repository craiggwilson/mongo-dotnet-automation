using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace MongoDB.Automation.Local
{
    public abstract class AbstractLocalBuilder<T> : ILocalInstanceProcessConfiguration
        where T : AbstractLocalBuilder<T>
    {
        private readonly Dictionary<string, string> _arguments;
        private string _binPath;

        public AbstractLocalBuilder()
        {
            _arguments = new Dictionary<string, string>();
        }

        string ILocalInstanceProcessConfiguration.BinPath
        {
            get { return _binPath; }
        }

        IEnumerable<KeyValuePair<string, string>> ILocalInstanceProcessConfiguration.Arguments
        {
            get { return _arguments; }
        }

        public T BindIPAddress(IPAddress ip)
        {
            return Set("bind_ip", ip.ToString());
        }

        public T BinPath(string binPath)
        {
            _binPath = binPath;
            return (T)this;
        }

        public LocalInstanceProcess Build()
        {
            if (_binPath == null)
            {
                throw new AutomationException("Must provide a binary path.");
            }

            return new LocalInstanceProcess(this);
        }

        public T Config(string configPath)
        {
            return Set("config", configPath);
        }

        public T IPV6()
        {
            return Set("ipv6");
        }

        public T KeyFile(string keyFilePath)
        {
            return Set("keyFile", keyFilePath);
        }

        public T LogAppend()
        {
            return Set("logappend");
        }

        public T LogPath(string logPath)
        {
            return Set("logpath", logPath);
        }

        public T MaxConnections(int maxConnections)
        {
            return Set("maxConn", maxConnections.ToString());
        }

        public T NoHttpInterface()
        {
            return Set("nohttpinterface");
        }

        public T Port(int port)
        {
            return Set("port", port.ToString());
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

            return Set(new String('v', count));
        }
    }
}