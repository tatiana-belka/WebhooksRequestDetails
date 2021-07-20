using Webhook.Channel;
using Webhook.Hubs;
using Webhook.Hubs.Payloads;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Webhook.Hubs.HubClients
{
    public interface IWebhookCoreHubClient : IHubClient
    {
        Task RequestBegin(RequestEventPayload payload);
        Task RequestEnd(RequestEventPayload payload);
    }
}
