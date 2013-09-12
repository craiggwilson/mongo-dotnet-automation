using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MongoDB.Automation
{
    public interface IShardableController : IController
    {
        /// <summary>
        /// Gets the address used to add the controller to a shard.
        /// </summary>
        string GetAddShardAddress();
    }
}