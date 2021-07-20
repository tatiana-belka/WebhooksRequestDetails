using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Webhook.Core;

namespace Webhook.Extensions
{
    public interface IRinBuilder
    {
        IServiceCollection Services { get; }
    }

    internal class WebhookBuilder : IRinBuilder
    {
        public IServiceCollection Services { get; }

        public WebhookBuilder(IServiceCollection services)
        {
            Services = services;
        }
    }
}
