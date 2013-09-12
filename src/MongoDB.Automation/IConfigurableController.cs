using MongoDB.Automation.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MongoDB.Automation
{
    public interface IConfigurableController : IController
    {
        /// <summary>
        /// Configures the controller.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        void Configure(IControllerConfiguration configuration);

        /// <summary>
        /// Gets the configuration.
        /// </summary>
        /// <returns>The configuration for the controller.</returns>
        IControllerConfiguration GetConfiguration();
    }
}
