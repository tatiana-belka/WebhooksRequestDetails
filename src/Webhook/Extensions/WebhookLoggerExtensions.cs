using Webhook.Logging;
using System;
using System.ComponentModel;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Extensions.Logging
{
    public static class WebhookLoggerExtensions
    {
        [Obsolete("Use AddWebhookLogger instead of UseWebhookLogger.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void UseWebhookLogger(this ILoggerFactory loggerFactory, IServiceProvider serviceProvider)
        {
            loggerFactory.AddProvider(new WebhookLoggerProvider(serviceProvider));
        }

        [Obsolete("Use AddWebhookLogger instead of UseWebhookLogger.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void UseWebhookLogger(this ILoggingBuilder loggingBuilder)
        {
            loggingBuilder.Services.AddSingleton<ILoggerProvider, WebhookLoggerProvider>();
        }

        public static void AddWebhookLogger(this ILoggerFactory loggerFactory, IServiceProvider serviceProvider)
        {
            loggerFactory.AddProvider(new WebhookLoggerProvider(serviceProvider));
        }

        public static void AddWebhookLogger(this ILoggingBuilder loggingBuilder)
        {
            loggingBuilder.Services.AddSingleton<ILoggerProvider, WebhookLoggerProvider>();
        }

    }
}
