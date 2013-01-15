using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MongoDB.Automation.Local
{
    public interface ILocalMongodBuilder<TBuilder> : ILocalBuilder<TBuilder>
        where TBuilder : ILocalMongodBuilder<TBuilder>
    {
        TBuilder DbPath(Func<LocalReplicaSetMember, string> dbPathFactory);

        TBuilder Ipv6();

        TBuilder NoJournal();
    }
}
