using System;
using Mod.Utility.Logging.Aws.Core;
using Microsoft.Extensions.Logging;
using Mod.Utility.Logging.Aws.Renderer;

namespace Mod.Utility.Logging.Aws
{
    /// <summary>
    /// AWS logger provider.
    /// </summary>
    /// <seealso cref="ILoggerProvider" />
    /// <seealso cref="ISupportExternalScope" />
    [ProviderAlias("AWS")]
    public class AwsLoggerProvider : ILoggerProvider, ISupportExternalScope
    {
        // To detect redundant calls
        private bool _disposed = false;

        /// <summary>
        /// The AWS logger options.
        /// </summary>
        private readonly AwsLoggerOptions awsLoggerOptions;

        private readonly ILogRenderer logRenderer;
        private readonly Lazy<IAwsLoggerCore> lazyCore;

        /// <summary>
        /// The external scope provider to allow setting scope data in messages.
        /// </summary>
        private IExternalScopeProvider externalScopeProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="AwsLoggerProvider"/> class.
        /// </summary>
        /// <param name="logRenderer">renderer for desired log format</param>
        /// <param name="awsLoggerOptions">The AWS logger options.</param>
        /// <exception cref="System.ArgumentNullException">
        /// telemetryConfiguration
        /// or
        /// loggingFilter
        /// or
        /// awsLoggerOptions.
        /// </exception>
        public AwsLoggerProvider(
            ILogRenderer logRenderer,
            AwsLoggerOptions awsLoggerOptions)
        {
            if (logRenderer == null)
            {
                throw new ArgumentNullException(nameof(logRenderer));
            }

            if (awsLoggerOptions == null)
            {
                throw new ArgumentNullException(nameof(awsLoggerOptions));
            }

            this.logRenderer = logRenderer;
            this.awsLoggerOptions = awsLoggerOptions;
            lazyCore = new Lazy<IAwsLoggerCore>(() => new AwsLoggerCore(this.awsLoggerOptions.Config, "ILogger"));
        }

        /// <summary>
        /// Creates a new <see cref="ILogger" /> instance.
        /// </summary>
        /// <param name="categoryName">The category name for messages produced by the logger.</param>
        /// <returns>An <see cref="ILogger"/> instance to be used for logging.</returns>
        public ILogger CreateLogger(string categoryName)
        {
            return new AwsLogger(
                    categoryName,
                    this.lazyCore,
                    this.logRenderer,
                    this.awsLoggerOptions)
            {
                ExternalScopeProvider = this.externalScopeProvider,
            };
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Sets the scope provider. This method also updates all the existing logger to also use the new ScopeProvider.
        /// </summary>
        /// <param name="externalScopeProvider">The external scope provider.</param>
        public void SetScopeProvider(IExternalScopeProvider externalScopeProvider)
        {
            this.externalScopeProvider = externalScopeProvider;
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources. 
        /// </summary>
        /// <param name="releasedManagedResources">Release managed resources.</param>
        protected virtual void Dispose(bool releasedManagedResources)
        {
            if (_disposed)
            {
                return;
            }

            if (releasedManagedResources)
            {
                // TODO: dispose managed state (managed objects).
                if (lazyCore.IsValueCreated)
                {
                    lazyCore.Value.Close();
                }
            }

            // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
            // TODO: set large fields to null.

            _disposed = true;
        }
    }
}
