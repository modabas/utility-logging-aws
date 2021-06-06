using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using Newtonsoft.Json;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Globalization;

namespace Mod.Utility.Logging.Aws.Renderer
{
    /// <summary>
    /// create JSON formatted log messages
    /// </summary>
    public class JsonRenderer : ILogRenderer
    {
        /// <summary>
        /// our serializer
        /// </summary>
        private readonly JsonSerializer _jss;

        /// <summary>
        /// cons, given a json serializer to hold on to
        /// </summary>
        /// <param name="jss">a JSON serializer to use per message</param>
        public JsonRenderer(JsonSerializer jss)
        {
            _jss = jss;
        }

        public string Render(IDictionary<string, object> logProperties, AwsLoggerOptions awsLoggerOptions)
        {
            var parameters = logProperties ?? new Dictionary<string, object>(0);

            var sb = new StringBuilder(256);
            using (var sw = new StringWriter(sb, CultureInfo.InvariantCulture))
            using (var jsonWriter = new JsonTextWriter(sw))
            {
                jsonWriter.Formatting = _jss.Formatting;

                _jss.Serialize(jsonWriter, parameters);

                return sw.ToString();
            }
        }
    }
}