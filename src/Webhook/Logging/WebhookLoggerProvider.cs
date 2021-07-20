using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Webhook.Features;

namespace Webhook.Logging
{
    internal class WebhookLoggerProvider : ILoggerProvider
    {
        private readonly IServiceProvider _serviceProvider;

        public WebhookLoggerProvider(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new WebhookLogger(_serviceProvider.GetService<IWebhookRequestRecordingFeatureAccessor>() ?? NullWebhookRequestRecordingFeatureAccessor.Instance);
        }

        public void Dispose()
        {
        }
    }
}
