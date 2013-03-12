using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MongoDB.Automation.Configuration
{
    public interface IControllerConfigurationPersister
    {
        IControllerConfiguration Load(Stream stream);

        void Save(IControllerConfiguration configuration, Stream stream);
    }
}