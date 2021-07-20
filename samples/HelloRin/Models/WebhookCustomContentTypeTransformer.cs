using Microsoft.Extensions.Primitives;
using Webhook.Core;
using Webhook.Core.Record;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MessagePack;
using static HelloWebhook.Controllers.MyApiController;

namespace HelloWebhook.Models
{
    public class WebhookCustomContentTypeTransformer : IBodyDataTransformer
    {
        public bool CanTransform(HttpRequestRecord record, StringValues contentTypeHeaderValues)
        {
            return contentTypeHeaderValues.Any(x => x == "application/x-msgpack");
        }

        public bool TryTransform(HttpRequestRecord record, ReadOnlySpan<byte> body, StringValues contentTypeHeaderValues, out BodyDataTransformResult result)
        {
            var json = MessagePackSerializer.ConvertToJson(body.ToArray(), MessagePack.Resolvers.ContractlessStandardResolver.Options);

            result = new BodyDataTransformResult(Encoding.UTF8.GetBytes(json), contentTypeHeaderValues.ToString(), "application/json");
            return true;
        }
    }
}
