using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MongoDB.Automation.Configuration
{
    public interface IConfigurationProvider
    {
        IConfiguration GetConfiguration();
    }
}