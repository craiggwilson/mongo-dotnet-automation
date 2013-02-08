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
            return Set(Constants.DB_PATH, dbPath);
        }

        public T NoJournal()
        {
            return Set(Constants.NO_JOURNAL);
        }

        public T NoPrealloc()
        {
            return Set(Constants.NO_PREALLOC);
        }

        public T SmallFiles()
        {
            return Set(Constants.SMALL_FILES);
        }
    }
}