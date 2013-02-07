using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MongoDB.Automation.Configuration
{
    public abstract class AbstractLocalMongodBuilder<T> : AbstractLocalConfigurationBuilder<T>
        where T : AbstractLocalMongodBuilder<T>
    {
        protected AbstractLocalMongodBuilder()
        { }

        protected AbstractLocalMongodBuilder(IEnumerable<KeyValuePair<string, string>> arguments)
            : base(arguments)
        { }

        public T DbPath(string dbPath)
        {
            return Set("dbpath", dbPath);
        }

        public T Ipv6()
        {
            return Set("ipv6");
        }

        public T NoJournal()
        {
            return Set("nojournal");
        }

        public T NoPrealloc()
        {
            return Set("noprealloc");
        }

        public T SmallFiles()
        {
            return Set("smallfiles");
        }
    }
}