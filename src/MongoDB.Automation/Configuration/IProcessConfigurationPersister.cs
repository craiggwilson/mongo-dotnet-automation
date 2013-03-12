using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MongoDB.Automation.Configuration
{
    public interface IProcessConfigurationPersister
    {
        IProcessConfiguration Load(Stream stream);

        void Save(IProcessConfiguration config, Stream stream);
    }
}