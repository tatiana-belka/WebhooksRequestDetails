using Microsoft.Extensions.Primitives;
using Webhook.Channel;
using Webhook.Core;
using Webhook.Core.Event;
using Webhook.Core.Record;
using Webhook.Hubs.HubClients;
using Webhook.Hubs.Payloads;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Webhook.Hubs
{
    public class WebhookCoreHub : IHub
    {
        private readonly IRecordStorage _storage;
        private readonly BodyDataTransformerSet _bodyDataTransformerSet;

        public WebhookCoreHub(IRecordStorage storage, BodyDataTransformerSet bodyDataTransformerSet)
        {
            _storage = storage;
            _bodyDataTransformerSet = bodyDataTransformerSet;
        }

        public async Task<RequestEventPayload[]> GetRecordingList()
        {
            return (await _storage.GetAllAsync()).Select(x => new RequestEventPayload(x)).ToArray();
        }

        public async Task<RequestRecordDetailPayload?> GetDetailById(string id)
        {
            var result = await _storage.TryGetDetailByIdAsync(id);

            return (result.Succeed && result.Value != null)
                ? new RequestRecordDetailPayload(result.Value)
                : null;
        }

        public async Task<BodyDataPayload?> GetRequestBody(string id)
        {
            var result = await _storage.TryGetDetailByIdAsync(id);
            var resultBody = await _storage.TryGetRequestBodyByIdAsync(id);

            return (result.Succeed && resultBody.Succeed && result.Value != null)
                ? BodyDataPayload.CreateFromRecord(result.Value, result.Value.RequestHeaders, resultBody.Value, _bodyDataTransformerSet.Request)
                : null;
        }

        public async Task<BodyDataPayload?> GetResponseBody(string id)
        {
            var result = await _storage.TryGetDetailByIdAsync(id);
            var resultBody = await _storage.TryGetResponseBodyByIdAsync(id);

            return (result.Succeed && resultBody.Succeed && result.Value != null)
                ? BodyDataPayload.CreateFromRecord(result.Value, result.Value.ResponseHeaders, resultBody.Value, _bodyDataTransformerSet.Response)
                : null;
        }

        public WebhookServerInfoPayload GetServerInfo()
        {
            return new WebhookServerInfoPayload(typeof(WebhookCoreHub).Assembly.GetName().Version!.ToString(), new FileInfo(typeof(WebhookCoreHub).Assembly.Location).LastWriteTimeUtc, Array.Empty<string>());
        }

        public bool Ping()
        {
            return true;
        }

        public class MessageSubscriber : IMessageSubscriber<RequestEventMessage>
        {
            private readonly IWebhookCoreHubClient _client;
            public MessageSubscriber(WebhookChannel channel)
            {
                _client = channel.GetClient<WebhookCoreHub, IWebhookCoreHubClient>();
            }

            public async Task Publish(RequestEventMessage message)
            {
                switch (message.Event)
                {
                    case RequestEvent.BeginRequest:
                        await _client.RequestBegin(new RequestEventPayload(message.Value));
                        break;
                    case RequestEvent.CompleteRequest:
                        await _client.RequestEnd(new RequestEventPayload(message.Value));
                        break;
                }
            }
        }
    }
}
