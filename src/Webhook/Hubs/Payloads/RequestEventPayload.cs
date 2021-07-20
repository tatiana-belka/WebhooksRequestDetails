using Webhook.Core;
using Webhook.Core.Record;
using System;

namespace Webhook.Hubs.Payloads
{
    public class RequestEventPayload
    {
        public string Id { get; }
        public string? ParentId { get; }
        public bool IsCompleted { get; }
        public DateTimeOffset RequestReceivedAt { get; }
        public string Method { get; }
        public string Path { get; }
        public int ResponseStatusCode { get; }

        public RequestEventPayload(HttpRequestRecordInfo record)
        {
            Id = record.Id;
            ParentId = record.ParentId;
            RequestReceivedAt = record.RequestReceivedAt;
            IsCompleted = record.IsCompleted;
            Method = record.Method;
            Path = record.Path;
            ResponseStatusCode = record.ResponseStatusCode;
        }
    }
}
