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
        private LocalDbPathOptions _dbPathOptions;

        protected LocalMongodBuilder(string binPath)
            : base(binPath)
        { }

        public TBuilder DbPath(Func<TSettings, string> dbPathFactory, LocalDbPathOptions options)
        {
            _dbPathOptions = options;
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
            var process = new LocalInstanceProcess<TSettings>(
                GetExecutable("mongod"),
                GetCommandArguments(settings),
                settings);
            
            string dbpath;
            if (TryGetArgument("dbpath", settings, out dbpath))
            {
                var exists = Directory.Exists(dbpath);
                if (_dbPathOptions == LocalDbPathOptions.CleanAndEnsureExists && exists)
                {
                    Config.Out.WriteLine("Removing directory at {0}", dbpath);
                    try
                    {
                        Directory.Delete(dbpath, true);
                    }
                    catch (Exception ex)
                    {
                        Config.Error.WriteLine("Unable to remove directory: {0}", ex.Message);
                        throw;
                    }
                    exists = false;
                }
                if ((_dbPathOptions == LocalDbPathOptions.CleanAndEnsureExists || _dbPathOptions == LocalDbPathOptions.EnsureExists) && !exists)
                {
                    try
                    {
                        Directory.CreateDirectory(dbpath);
                    }
                    catch (Exception ex)
                    {
                        Config.Error.WriteLine("Unable to create directory: {0}", ex.Message);
                        throw;
                    }
                }
            }

            return process;
        }
    }
}