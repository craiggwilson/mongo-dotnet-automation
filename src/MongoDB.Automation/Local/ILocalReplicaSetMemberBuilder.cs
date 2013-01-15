using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MongoDB.Automation.Local
{
    public interface ILocalReplicaSetMemberBuilder : ILocalMongodBuilder<ILocalReplicaSetMemberBuilder>
    {
        ILocalReplicaSetMemberBuilder OplogSize(int size);
    }
}
