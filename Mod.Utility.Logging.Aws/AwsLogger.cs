namespace Mod.Utility.Logging.Aws
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Text;
    using Microsoft.Extensions.Logging;
    using Mod.Utility.Logging.Aws.Core;
    using Mod.Utility.Logging.Aws.Renderer;

    /// <summary>
    /// AWS logger implementation for <see cref="ILogger"/>.
    /// </summary>
    /// <seealso cref="ILogger" />
    public class AwsLogger : ILogger
    {
        private readonly string categoryName;
        private readonly Lazy<IAwsLoggerCore> lazyCore;
        private readonly ILogRenderer logRenderer;
        private readonly AwsLoggerOptions awsLoggerOptions;

        /// <summary>
        /// Creates a new instance of <see cref="AwsLogger"/>.
        /// </summary>
        public AwsLogger(
            string categoryName,
            Lazy<IAwsLoggerCore> lazyCore,
            ILogRenderer logRenderer,
            AwsLoggerOptions awsLoggerOptions)
        {
            this.categoryName = categoryName;
            this.lazyCore = lazyCore;
            this.logRenderer = logRenderer;
            this.awsLoggerOptions = awsLoggerOptions ?? throw new ArgumentNullException(nameof(awsLoggerOptions));
        }

        /// <summary>
        /// Gets or sets the external scope provider.
        /// </summary>
        internal IExternalScopeProvider ExternalScopeProvider { get; set; }

        /// <summary>
        /// Begins a logical operation scope.
        /// </summary>
        /// <typeparam name="TState">Current state.</typeparam>
        /// <param name="state">The identifier for the scope.</param>
        /// <returns>
        /// An IDisposable that ends the logical operation scope on dispose.
        /// </returns>
        public IDisposable BeginScope<TState>(TState state)
        {
            return this.ExternalScopeProvider != null ? this.ExternalScopeProvider.Push(state) : NullScope.Instance;
        }

        /// <summary>
        /// Checks if the given <paramref name="logLevel" /> is enabled.
        /// </summary>
        /// <param name="logLevel">level to be checked.</param>
        /// <returns>
        ///   <c>true</c> if enabled.
        /// </returns>
        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel != LogLevel.None;
        }

        /// <summary>
        /// Writes a log entry.
        /// </summary>
        /// <typeparam name="TState">State being passed along.</typeparam>
        /// <param name="logLevel">Entry will be written on this level.</param>
        /// <param name="eventId">Id of the event.</param>
        /// <param name="state">The entry to be written. Can be also an object.</param>
        /// <param name="exception">The exception related to this entry.</param>
        /// <param name="formatter">Function to create a <c>string</c> message of the <paramref name="state" /> and <paramref name="exception" />.</param>
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            if (formatter == null)
            {
                throw new ArgumentNullException(nameof(formatter));
            }

            var logMessage = formatter(state, exception);
            var logProperties = PopulateProperties(logLevel, logMessage, state, exception, eventId);

            var renderedLog = logRenderer.Render(logProperties, awsLoggerOptions);
            lazyCore.Value.AddMessage(renderedLog);
            //Console.WriteLine(renderedLog);
        }

        private IDictionary<string, object> PopulateProperties<TState>(LogLevel logLevel, string logMessage, TState state, Exception exception, EventId eventId)
        {
            IDictionary<string, object> dict = new Dictionary<string, object>();

            if (awsLoggerOptions.IncludeLogLevel)
                dict[RendererConstants.LOG_LEVEL_KEY] = logLevel;
            if (awsLoggerOptions.IncludeCategory)
                dict[RendererConstants.CATEGORY_NAME_KEY] = categoryName;
            if (awsLoggerOptions.IncludeEventId)
            {
                if (eventId.Id != 0)
                {
                    dict[RendererConstants.EVENT_ID_KEY] = eventId.Id.ToString(CultureInfo.InvariantCulture);
                }
                if (!string.IsNullOrEmpty(eventId.Name))
                {
                    dict[RendererConstants.EVENT_NAME_KEY] = eventId.Name;
                }
            }

            dict[RendererConstants.MESSAGE_KEY] = logMessage;

            if (awsLoggerOptions.IncludeException)
            {
                if (exception != null)
                {
                    dict[RendererConstants.EXCEPTION_MESSAGE_KEY] = exception.Message;
                    dict[RendererConstants.EXCEPTION_KEY] = exception;
                }
            }

            if (awsLoggerOptions.IncludeSemantics)
            {
                if (state is IReadOnlyCollection<KeyValuePair<string, object>> stateDictionary)
                {
                    dict[RendererConstants.SEMANTICS_KEY] = new Dictionary<string, object>();
                    foreach (KeyValuePair<string, object> item in stateDictionary)
                    {
                        ((Dictionary<string, object>)dict[RendererConstants.SEMANTICS_KEY])[item.Key] = item.Value;
                    }
                }
            }

            if (awsLoggerOptions.IncludeScopes)
            {
                dict[RendererConstants.SCOPE_KEY] = new Dictionary<string, object>();
                if (this.ExternalScopeProvider != null)
                {
                    StringBuilder stringBuilder = new StringBuilder();
                    this.ExternalScopeProvider.ForEachScope(
                        (activeScope, builder) =>
                        {
                            // Ideally we expect that the scope to implement IReadOnlyList<KeyValuePair<string, object>>.
                            // But this is not guaranteed as user can call BeginScope and pass anything. Hence
                            // we try to resolve the scope as Dictionary and if we fail, we just serialize the object and add it.

                            if (activeScope is IReadOnlyCollection<KeyValuePair<string, object>> activeScopeDictionary)
                            {
                                foreach (KeyValuePair<string, object> item in activeScopeDictionary)
                                {
                                    ((Dictionary<string, object>)dict[RendererConstants.SCOPE_KEY])[item.Key] = item.Value;
                                }
                            }
                            else
                            {
                                builder.Append(" => ").Append(activeScope);
                            }
                        },
                        stringBuilder);

                    if (stringBuilder.Length > 0)
                    {
                        ((Dictionary<string, object>)dict[RendererConstants.SCOPE_KEY])[RendererConstants.SCOPE_APPENDED_TEXT_KEY] = stringBuilder.ToString();
                    }
                }
            }
            return dict;
        }
    }
}
