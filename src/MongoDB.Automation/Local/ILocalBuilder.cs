using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace MongoDB.Automation.Local
{
    public interface ILocalBuilder<TBuilder>
        where TBuilder : ILocalBuilder<TBuilder>
    {
        TBuilder BindIPAddress(IPAddress ip);

        TBuilder BinPath(string binPath);

        LocalInstanceProcess Build();

        TBuilder Config(string configPath);

        TBuilder IPV6();

        TBuilder KeyFile(string keyFilePath);

        TBuilder LogAppend();

        TBuilder LogPath(string logPath);

        TBuilder MaxConnections(int maxConnections);

        TBuilder NoHttpInterface();

        TBuilder Port(int port);

        TBuilder Set(string name);

        TBuilder Set(string name, string value);

        TBuilder Verbosity(int count);
    }
}