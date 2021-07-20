using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Webhook.Channel;
using Webhook.Core;
using Webhook.Core.Event;
using Webhook.Core.Record;
using Webhook.Extensions;
using Webhook.Features;
using Webhook.Hubs;
using Webhook.Hubs.HubClients;
using Webhook.Hubs.Payloads;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Features;

namespace Webhook.Middlewares
{
    public class RequestRecorderMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IMessageEventBus<RequestEventMessage> _eventBus;
        private readonly IMessageEventBus<StoreBodyEventMessage> _eventBusStoreBody;
        private readonly IWebhookRequestRecordingFeatureAccessor _recordingFeatureAccessor;
        private readonly ILogger _logger;

        public const string EventSourceName = "Webhook.Middlewares.RequestRecorderMiddleware";

        public RequestRecorderMiddleware(
            RequestDelegate next,
            IMessageEventBus<RequestEventMessage> eventBus,
            IMessageEventBus<StoreBodyEventMessage> eventBusStoreBody,
            WebhookChannel rinChannel,
            ILoggerFactory loggerFactory,
            IWebhookRequestRecordingFeatureAccessor recordingFeatureAccessor)
        {
            _next = next;
            _eventBus = eventBus;
            _eventBusStoreBody = eventBusStoreBody;
            _logger = loggerFactory.CreateLogger<RequestRecorderMiddleware>();
            _recordingFeatureAccessor = recordingFeatureAccessor;
        }

        public async Task InvokeAsync(HttpContext context, WebhookOptions options)
        {
            var request = context.Request;
            var response = context.Response;

            if (request.Path.StartsWithSegments(options.Inspector.MountPath) || (options.RequestRecorder.Excludes.Any(x => x.Invoke(request))))
            {
                await _next(context);
                return;
            }

            
            var timelineRoot = TimelineScope.Prepare();
            _recordingFeatureAccessor.SetValue(null);

            HttpRequestRecord? record = default;
            try
            {
                record = await PreprocessAsync(context, options, timelineRoot);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled Exception was thrown until pre-processing");
            }
            
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                if (record != null)
                {
                    record.Exception = new ExceptionData(ex);
                }
                throw;
            }
            finally
            {
                try
                {
                    if (record != null)
                    {
                        await PostprocessAsync(context, options, record);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unhandled Exception was thrown until post-processing");
                }
            }
        }

        private async Task<HttpRequestRecord> PreprocessAsync(HttpContext context, WebhookOptions options, ITimelineScope timelineRoot)
        {
            var request = context.Request;
            var response = context.Response;

            var record = new HttpRequestRecord()
            {
                Id = Guid.NewGuid().ToString(),
                IsHttps = request.IsHttps,
                Host = request.Host.Value,
                QueryString = request.QueryString.Value,
                Path = request.Path,
                Method = request.Method,
                RequestReceivedAt = DateTimeOffset.Now,
                RequestHeaders = request.Headers.ToDictionary(k => k.Key, v => v.Value),
                RemoteIpAddress = request.HttpContext.Connection.RemoteIpAddress,
                Timeline = timelineRoot,
            };

            
            var feature = new WebhookRequestRecordingFeature(record);;
            _recordingFeatureAccessor.SetValue(feature);
            context.Features.Set<IWebhookRequestRecordingFeature>(feature);

            await _eventBus.PostAsync(new RequestEventMessage(EventSourceName, record, RequestEvent.BeginRequest));

            
            context.Response.Headers.Add("X-Webhook-Request-Id", record.Id);

            if (options.RequestRecorder.EnableBodyCapturing)
            {
                context.EnableResponseDataCapturing();
                request.EnableBuffering();
            }
            response.OnStarting(OnStarting, record);
            response.OnCompleted(OnCompleted, record);

            
            record.Processing = TimelineScope.Create("Processing", TimelineEventCategory.AspNetCoreCommon);

            return record;
        }

        private async Task PostprocessAsync(HttpContext context, WebhookOptions options, HttpRequestRecord record)
        {
            var request = context.Request;
            var response = context.Response;

            record.Processing.Complete();

            record.ResponseStatusCode = response.StatusCode;
            record.ResponseHeaders = response.Headers.ToDictionary(k => k.Key, v => v.Value);

            if (options.RequestRecorder.EnableBodyCapturing)
            {
                var feature = context.Features.Get<IWebhookRequestRecordingFeature>();

                var memoryStreamRequestBody = new MemoryStream();
                request.Body.Position = 0; 
                await request.Body.CopyToAsync(memoryStreamRequestBody);

                await _eventBusStoreBody.PostAsync(new StoreBodyEventMessage(StoreBodyEvent.Request, record.Id, memoryStreamRequestBody.ToArray()));
                if (feature.ResponseDataStream != null)
                {
                    await _eventBusStoreBody.PostAsync(new StoreBodyEventMessage(StoreBodyEvent.Response, record.Id, feature.ResponseDataStream.GetCapturedData()));
                }
            }

            if (request.CheckTrailersAvailable())
            {
                var trailers = context.Features.Get<IHttpRequestTrailersFeature>();
                record.RequestTrailers = trailers.Trailers.ToDictionary(k => k.Key, v => v.Value);
            }
            if (response.SupportsTrailers())
            {
                var trailers = context.Features.Get<IHttpResponseTrailersFeature>();
                record.ResponseTrailers = trailers.Trailers.ToDictionary(k => k.Key, v => v.Value);
            }

            var exceptionFeature = context.Features.Get<IExceptionHandlerFeature>();
            if (exceptionFeature != null)
            {
                record.Exception = new ExceptionData(exceptionFeature.Error);
            }
        }

        private Task OnStarting(object state)
        {
            var record = ((HttpRequestRecord)state);
            record.Transferring = TimelineScope.Create("Transferring", TimelineEventCategory.AspNetCoreCommon);
            return Task.CompletedTask;
        }

        private Task OnCompleted(object state)
        {
            var record = ((HttpRequestRecord)state);

            record.TransferringCompletedAt = DateTime.Now;
            record.Transferring?.Complete();
            record.Timeline.Complete();

            return _eventBus.PostAsync(new RequestEventMessage(EventSourceName, record, RequestEvent.CompleteRequest)).AsTask();
        }
    }
}
