using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Mod.Utility.Logging.Aws.Core;

namespace Mod.Utility.Logging.Aws.Renderer
{
    /// <summary>
    /// Renderer interface
    /// </summary>
    public interface ILogRenderer
    {
        /// <summary>
        /// Render a log message into any format converted to string
        /// </summary>
        /// <param name="logLevel">Log Level</param>
        /// <param name="logMessage">The previously rendered message. Can be used as a starting point to augment.</param>
        /// <param name="exception">Exception, if applicable</param>
        /// <param name="logProperties">Any additional parameters like eventId, scope, etc</param>
        /// <returns>The newly render message used to replace the original</returns>
        string Render(IDictionary<string, object> logProperties, AwsLoggerOptions awsLoggerOptions);

    }
}
