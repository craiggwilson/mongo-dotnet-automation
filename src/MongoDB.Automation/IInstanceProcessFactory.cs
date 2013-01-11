using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MongoDB.Automation
{
    public interface IInstanceProcessFactory<TSettings> where TSettings : IInstanceProcessSettings
    {
        IInstanceProcess<TSettings> Create(TSettings settings);
    }
}