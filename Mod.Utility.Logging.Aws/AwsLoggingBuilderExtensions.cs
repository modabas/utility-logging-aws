namespace Microsoft.Extensions.Logging
{
    using System;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Mod.Utility.Logging.Aws;
    using Mod.Utility.Logging.Aws.Converters;
    using Mod.Utility.Logging.Aws.Core;
    using Mod.Utility.Logging.Aws.Renderer;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    /// <summary>
    /// Extensions methods to add and configure AWS logger.
    /// </summary>
    public static class AwsLoggingBuilderExtensions
    {
        internal static JsonSerializerSettings JsonSettings
        {
            get
            {
                var result = new JsonSerializerSettings()
                {
                    Formatting = Formatting.None,
                    PreserveReferencesHandling = PreserveReferencesHandling.None,
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    NullValueHandling = NullValueHandling.Ignore,
                    TypeNameHandling = TypeNameHandling.None
                };

                result.Converters.Add(new StringEnumConverter());
                result.Converters.Add(new ExceptionStringConverter());
                result.Converters.Add(new IPAddressConverter());
                result.Converters.Add(new IPEndPointConverter());
                return result;
            }
        }

        /// <summary>
        /// lazy jss access
        /// </summary>
        private static readonly Lazy<JsonSerializer> _lazyjss = new Lazy<JsonSerializer>(() => JsonSerializer.Create(JsonSettings));


        /// <summary>
        /// Wires up the Logging Builder to AWS Logging, specifically using the StructuredRenderer
        /// </summary>
        /// <param name="loggingBuilder">dotnet Logging Builder</param>
        /// <param name="configSection">configuration root in which we are looking for the block specified by configSectionInfoBlockName</param>
        /// <param name="configSectionInfoBlockName">config section info block name, ex. AWS.Logging or AWS</param>
        /// <param name="awsLoggerConfigAction">optional aws logger reconfigurator action</param>
        /// <returns>The same Logging Builder that was passed in</returns>
        public static ILoggingBuilder AddAwsLogging(this ILoggingBuilder loggingBuilder, AwsLoggerOptions config, ILogRenderer logRenderer = null, Action<AwsLoggerConfig> awsLoggerConfigAction = null)
        {
            if (loggingBuilder == null)
            {
                throw new ArgumentNullException(nameof(loggingBuilder));
            }
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            var _logRenderer = logRenderer;
            if (_logRenderer == null)
                _logRenderer = new JsonRenderer(_lazyjss.Value);

            awsLoggerConfigAction?.Invoke(config.Config);
            var awsProvider = new AwsLoggerProvider(_logRenderer, config);
            loggingBuilder.AddProvider(awsProvider); 

            //Return the same Logging Builder that was passed in
            return loggingBuilder;
        }

    }
}
