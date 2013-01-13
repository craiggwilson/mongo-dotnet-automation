using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MongoDB.Automation
{
    public abstract class LocalMongodBuilder<TBuilder, TSettings> : LocalBuilder<TBuilder, TSettings>, IInstanceProcessFactory<TSettings>
        where TBuilder : LocalMongodBuilder<TBuilder, TSettings>
        where TSettings : IInstanceProcessSettings
    {
        protected LocalMongodBuilder(string binPath)
            : base(binPath)
        { }

        public TBuilder DbPath(Func<TSettings, string> dbPathFactory)
        {
            return Set("dbpath", dbPathFactory);
        }

        public TBuilder Ipv6()
        {
            return Set("ipv6", (string)null);
        }

        public TBuilder LogPath(Func<TSettings, string> logPathFactory)
        {
            return Set("logpath", logPathFactory);
        }

        public TBuilder NoJournal()
        {
            return Set("nojournal", (string)null);
        }

        public IInstanceProcess<TSettings> Create(TSettings settings)
        {
            string dbpath;
            TryGetArgument("dbpath", settings, out dbpath);
            string logpath;
            TryGetArgument("logpath", settings, out logpath);
            return new LocalInstanceProcess<TSettings>(
                GetExecutable("mongod"),
                GetCommandArguments(settings),
                settings,
                dbpath,
                logpath);
        }
    }
}