using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Webhook.Core;
using Webhook.Core.Event;
using Webhook.Core.Record;
using Webhook.Middlewares;
using Webhook.Middlewares.Api;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace Microsoft.AspNetCore.Builder
{
    public static class WebhookMiddlewareExtensions
    {
        public static void UseWebhook(this IApplicationBuilder app)
        {
            var options = app.ApplicationServices.GetRequiredService<WebhookOptions>();
            if (options == null)
            {
                throw new InvalidOperationException("Webhook Services are not registered. Please call 'services.AddRin()' in a Startup class");
            }

            var env = app.ApplicationServices.GetRequiredService<IHostEnvironment>();
            if (env.IsProduction() && !options.RequestRecorder.AllowRunningOnProduction)
            {
                throw new InvalidOperationException("Webhook requires non-Production environment to run. If you want to run in Production environment, configure AllowRunningOnProduction option.");
            }

            app.UseWebhookMessageBus();

            app.UseWebhookInspector();
            app.UseWebhookRecorder();
        }

        private static void UseWebhookMessageBus(this IApplicationBuilder app)
        {
            var eventBus = app.ApplicationServices.GetRequiredService<IMessageEventBus<RequestEventMessage>>();
            var eventBusStoreBody = app.ApplicationServices.GetRequiredService<IMessageEventBus<StoreBodyEventMessage>>();

            var subscribers = app.ApplicationServices.GetServices<IMessageSubscriber<RequestEventMessage>>() ?? Enumerable.Empty<IMessageSubscriber<RequestEventMessage>>();
            var subscribersStoreBody = app.ApplicationServices.GetServices<IMessageSubscriber<StoreBodyEventMessage>>() ?? Enumerable.Empty<IMessageSubscriber<StoreBodyEventMessage>>();
            var recorder = app.ApplicationServices.GetRequiredService<IRecordStorage>();

            eventBus.Subscribe(subscribers.Concat(new[] { recorder }));
            eventBusStoreBody.Subscribe(subscribersStoreBody.Concat(new[] { recorder }));
        }

        private static void UseWebhookInspector(this IApplicationBuilder app)
        {
            var options = app.ApplicationServices.GetRequiredService<WebhookOptions>();

            app.Map(options.Inspector.MountPath, branch =>
            {
                branch.UseWebSockets();
                branch.UseWebhookApi();
                branch.UseWebhookInspectorApi();
                branch.UseWebhookDownloadEndpoints();

                // ResourcesMiddleware must be last at pipeline.
                branch.UseWebhookEmbeddedResources();
            });
        }

        private static void UseWebhookDownloadEndpoints(this IApplicationBuilder app)
        {
            app.Map("/download/request", x => x.UseMiddleware<DownloadRequestBodyMiddleware>());
            app.Map("/download/response", x => x.UseMiddleware<DownloadResponseBodyMiddleware>());
        }

        private static void UseWebhookEmbeddedResources(this IApplicationBuilder app)
        {
            app.UseMiddleware<ResourcesMiddleware>();
        }

        private static void UseWebhookInspectorApi(this IApplicationBuilder app)
        {
            app.Map("/chan", x => x.UseMiddleware<ChannelMiddleware>());
        }

        private static void UseWebhookApi(this IApplicationBuilder app)
        {
            app.Map("/api/GetDetailById", x => x.UseMiddleware<GetDetailByIdMiddleware>());
        }

        private static void UseWebhookRecorder(this IApplicationBuilder app)
        {
            app.UseMiddleware<RequestRecorderMiddleware>();
        }

        public static void UseWebhookDiagnosticsHandler(this IApplicationBuilder app)
        {
            app.UseMiddleware<DiagnosticsMiddleware>();
        }
    }
}
