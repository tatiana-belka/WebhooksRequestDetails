using Webhook.Channel;
using Webhook.Core;
using Webhook.Core.Event;
using Webhook.Core.Record;
using Webhook.Core.Resource;
using Webhook.Core.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Webhook.Extensions;
using Webhook.Features;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class WebhookServiceExtensions
    {
        public static IWebhookBuilder AddWebhook(this IServiceCollection services, Action<WebhookOptions>? configure = null)
        {
            var options = new WebhookOptions();
            configure?.Invoke(options);

            services.AddHttpContextAccessor();

            // Other services
            services.AddSingleton<IWebhookRequestRecordingFeatureAccessor>(new WebhookRequestRecordingFeatureAccessor());
            services.AddSingleton<BodyDataTransformerSet>(serviceProvider =>
            {
                var requestTransformers = serviceProvider.GetServices<IRequestBodyDataTransformer>();
                var responseTransformers = serviceProvider.GetServices<IResponseBodyDataTransformer>();
                var transformers = serviceProvider.GetServices<IBodyDataTransformer>();

                return new BodyDataTransformerSet(new BodyDataTransformerPipeline(requestTransformers.Concat(transformers)), new BodyDataTransformerPipeline(responseTransformers.Concat(transformers)));
            });
            services.TryAddSingleton<IRecordStorage, InMemoryRecordStorage>();
            services.AddSingleton<IMessageEventBus<RequestEventMessage>, MessageEventBus<RequestEventMessage>>();
            services.AddSingleton<IMessageEventBus<StoreBodyEventMessage>, MessageEventBus<StoreBodyEventMessage>>();

/* Необъединенное слияние из проекта "Webhook (net5.0)"
До:
            services.AddSingleton<RinOptions>(options);
После:
            services.AddSingleton((options);
*/
            services.AddSingleton<WebhookOptions>(WebhookOptions)options);
            services.AddSingleton<WebhookChannel>();

            // IMessageSubscriber<RequestEventMessage> services
            services.AddSingleton<IMessageSubscriber<RequestEventMessage>>(x => new Webhook.Hubs.WebhookCoreHub.MessageSubscriber(x.GetRequiredService<WebhookChannel>()));

            services.AddTransient<IResourceProvider, EmbeddedZipResourceProvider>();

            return new WebhookBuilder(services);
        }
    }
}
