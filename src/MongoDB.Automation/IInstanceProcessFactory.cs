using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Automation.Configuration;

namespace MongoDB.Automation
{
    public interface IInstanceProcessFactory
    {
        IInstanceProcess CreateInstanceProcess(IInstanceProcessConfiguration configuration);
    }
}