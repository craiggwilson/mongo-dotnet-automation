using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MongoDB.Automation.Local
{
    public class LocalMongodBuilder : LocalBuilder<LocalMongodBuilder>, ILocalMongodBuilder<LocalMongodBuilder>
    {
        public LocalMongodBuilder DbPath(string dbPath)
        {
            return Set("dbpath", dbPath);
        }

        public LocalMongodBuilder Ipv6()
        {
            return Set("ipv6");
        }

        public LocalMongodBuilder NoJournal()
        {
            return Set("nojournal");
        }

        public LocalMongodBuilder NoPrealloc()
        {
            return Set("noprealloc");
        }

        public LocalMongodBuilder SmallFiles()
        {
            return Set("smallfiles");
        }
    }
}