using Microsoft.Extensions.Configuration;
using Mod.Utility.Logging.Aws.Core;
using System;

namespace Mod.Utility.Logging.Aws
{
    /// <summary>
    /// Extensions methods for IConfiguration to lookup AWS logger configuration
    /// </summary>
    public static class ConfigurationSectionExtensions
    {
        // Default configuration block on the appsettings.json
        // Customer's information will be fetched from this block.
        private const string DEFAULT_BLOCK = "Logging:AWS";

        /// <summary>
        /// Loads the AWS Logger Configuration from the ConfigSection
        /// </summary>
        /// <param name="configSection">ConfigSection</param>
        /// <param name="configSectionInfoBlockName">ConfigSection SubPath to load from</param>
        /// <returns></returns>
        public static AwsLoggerOptions GetAwsLoggingConfigSection(this IConfiguration configSection, string configSectionInfoBlockName = DEFAULT_BLOCK)
        {
            var loggerConfigSection = configSection.GetSection(configSectionInfoBlockName);
            AwsLoggerOptions configObj = null;
            if (loggerConfigSection[AwsLoggerOptions.LOG_GROUP] != null)
            {
                configObj = new AwsLoggerOptions(loggerConfigSection);
            }

            return configObj;
        }
    }

    /// <summary>
    /// <see cref="AwsLoggerOptions"/> defines the custom behavior of the tracing information sent to AWS.
    /// </summary>
    public class AwsLoggerOptions
    {
        /// <summary>
        /// Gets or sets a value indicating whether the Scope information is included from telemetry or not.
        /// Defaults to true.
        /// </summary>
        public bool IncludeScopes { get; set; } = true;

        /// <summary>
        /// This determines if log level is included in a log message.
        /// <para>
        /// The default is true.
        /// </para>
        /// </summary>
        public bool IncludeLogLevel { get; set; } = true;

        /// <summary>
        /// This determines if category is included in a log message.
        /// <para>
        /// The default is true.
        /// </para>
        /// </summary>
        public bool IncludeCategory { get; set; } = true;

        /// <summary>
        /// This determines if event id is included in a log message.
        /// <para>
        /// The default is true.
        /// </para>
        /// </summary>
        public bool IncludeEventId { get; set; } = true;

        /// <summary>
        /// This determines if a new line is added to the end of the log message. Not used for default JsonFormatter.
        /// <para>
        /// The default is true.
        /// </para>
        /// </summary>
        public bool IncludeNewline { get; set; } = true;

        /// <summary>
        /// This determines if log message parameters are included as seperate fields.
        /// <para>
        /// The default is true.
        /// </para>
        /// </summary>
        public bool IncludeSemantics { get; set; } = true;

        /// <summary>
        /// This determines if exceptions are included in a log message.
        /// <para>
        /// The default is true.
        /// </para>
        /// </summary>
        public bool IncludeException { get; set; } = true;

        /// <summary>
        /// Configuration options for logging messages to AWS
        /// </summary>
        public AwsLoggerConfig Config { get; set; } = new AwsLoggerConfig();

        internal const string LOG_GROUP = "LogGroup";
        internal const string DISABLE_LOG_GROUP_CREATION = "DisableLogGroupCreation";
        internal const string REGION = "Region";
        internal const string SERVICEURL = "ServiceUrl";
        internal const string PROFILE = "Profile";
        internal const string PROFILE_LOCATION = "ProfilesLocation";
        internal const string BATCH_PUSH_INTERVAL = "BatchPushInterval";
        internal const string BATCH_PUSH_SIZE_IN_BYTES = "BatchPushSizeInBytes";
        internal const string LOG_LEVEL = "LogLevel";
        internal const string MAX_QUEUED_MESSAGES = "MaxQueuedMessages";
        internal const string LOG_STREAM_NAME_SUFFIX = "LogStreamNameSuffix";
        internal const string LOG_STREAM_NAME_PREFIX = "LogStreamNamePrefix";
        internal const string LIBRARY_LOG_FILE_NAME = "LibraryLogFileName";

        private const string INCLUDE_LOG_LEVEL_KEY = "IncludeLogLevel";
        private const string INCLUDE_CATEGORY_KEY = "IncludeCategory";
        private const string INCLUDE_NEWLINE_KEY = "IncludeNewline";
        private const string INCLUDE_EXCEPTION_KEY = "IncludeException";
        private const string INCLUDE_EVENT_ID_KEY = "IncludeEventId";
        private const string INCLUDE_SCOPES_KEY = "IncludeScopes";
        private const string INCLUDE_SEMANTICS_KEY = "IncludeSemantics";

        /// <summary>
        /// Construct an instance of AWSLoggerConfigSection
        /// </summary>
        /// <param name="loggerConfigSection">ConfigSection to parse</param>
        public AwsLoggerOptions(IConfiguration loggerConfigSection)
        {
            Config.LogGroup = loggerConfigSection[LOG_GROUP];
            Config.DisableLogGroupCreation = loggerConfigSection.GetValue<bool>(DISABLE_LOG_GROUP_CREATION);
            if (loggerConfigSection[REGION] != null)
            {
                Config.Region = loggerConfigSection[REGION];
            }
            if (loggerConfigSection[SERVICEURL] != null)
            {
                Config.ServiceUrl = loggerConfigSection[SERVICEURL];
            }
            if (loggerConfigSection[PROFILE] != null)
            {
                Config.Profile = loggerConfigSection[PROFILE];
            }
            if (loggerConfigSection[PROFILE_LOCATION] != null)
            {
                Config.ProfilesLocation = loggerConfigSection[PROFILE_LOCATION];
            }
            if (loggerConfigSection[BATCH_PUSH_INTERVAL] != null)
            {
                Config.BatchPushInterval = TimeSpan.FromMilliseconds(Int32.Parse(loggerConfigSection[BATCH_PUSH_INTERVAL]));
            }
            if (loggerConfigSection[BATCH_PUSH_SIZE_IN_BYTES] != null)
            {
                Config.BatchSizeInBytes = Int32.Parse(loggerConfigSection[BATCH_PUSH_SIZE_IN_BYTES]);
            }
            if (loggerConfigSection[MAX_QUEUED_MESSAGES] != null)
            {
                Config.MaxQueuedMessages = Int32.Parse(loggerConfigSection[MAX_QUEUED_MESSAGES]);
            }
            if (loggerConfigSection[LOG_STREAM_NAME_SUFFIX] != null)
            {
                Config.LogStreamNameSuffix = loggerConfigSection[LOG_STREAM_NAME_SUFFIX];
            }
            if (loggerConfigSection[LOG_STREAM_NAME_PREFIX] != null)
            {
                Config.LogStreamNamePrefix = loggerConfigSection[LOG_STREAM_NAME_PREFIX];
            }
            if (loggerConfigSection[LIBRARY_LOG_FILE_NAME] != null)
            {
                Config.LibraryLogFileName = loggerConfigSection[LIBRARY_LOG_FILE_NAME];
            }

            if (loggerConfigSection[INCLUDE_LOG_LEVEL_KEY] != null)
            {
                this.IncludeLogLevel = Boolean.Parse(loggerConfigSection[INCLUDE_LOG_LEVEL_KEY]);
            }
            if (loggerConfigSection[INCLUDE_CATEGORY_KEY] != null)
            {
                this.IncludeCategory = Boolean.Parse(loggerConfigSection[INCLUDE_CATEGORY_KEY]);
            }
            if (loggerConfigSection[INCLUDE_NEWLINE_KEY] != null)
            {
                this.IncludeNewline = Boolean.Parse(loggerConfigSection[INCLUDE_NEWLINE_KEY]);
            }
            if (loggerConfigSection[INCLUDE_EXCEPTION_KEY] != null)
            {
                this.IncludeException = Boolean.Parse(loggerConfigSection[INCLUDE_EXCEPTION_KEY]);
            }
            if (loggerConfigSection[INCLUDE_EVENT_ID_KEY] != null)
            {
                this.IncludeEventId = Boolean.Parse(loggerConfigSection[INCLUDE_EVENT_ID_KEY]);
            }
            if (loggerConfigSection[INCLUDE_SCOPES_KEY] != null)
            {
                this.IncludeScopes = Boolean.Parse(loggerConfigSection[INCLUDE_SCOPES_KEY]);
            }
            if (loggerConfigSection[INCLUDE_SEMANTICS_KEY] != null)
            {
                this.IncludeSemantics = Boolean.Parse(loggerConfigSection[INCLUDE_SEMANTICS_KEY]);
            }
        }
    }
}