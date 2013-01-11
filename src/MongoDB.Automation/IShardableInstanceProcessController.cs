using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MongoDB.Automation
{
    public interface IShardableInstanceProcessController : IInstanceProcessController
    {
        /// <summary>
        /// Gets the address used to add the process controller to a shard.
        /// </summary>
        string GetAddShardAddress();
    }
}