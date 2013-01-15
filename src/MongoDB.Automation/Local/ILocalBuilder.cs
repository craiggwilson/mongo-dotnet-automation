using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MongoDB.Automation.Local
{
    public interface ILocalBuilder<TBuilder>
        where TBuilder : ILocalBuilder<TBuilder>
    {
        TBuilder BinPath(string binPath);

        TBuilder LogPath(string logPath);

        TBuilder Set(string name, string value);
    }
}