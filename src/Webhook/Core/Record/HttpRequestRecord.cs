using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Webhook.Core.Record
{
    public class HttpRequestRecordInfo
    {
        public string Id { get; set; } = default!;
        public string? ParentId { get; set; }

        public bool IsHttps { get; set; }
        public string Host { get; set; } = default!;
        public string? QueryString { get; set; }
        public string Path { get; set; } = default!;
        public string Method { get; set; } = default!;
        public int ResponseStatusCode { get; set; }
        public IPAddress? RemoteIpAddress { get; set; }

        public DateTimeOffset RequestReceivedAt { get; set; }
        public DateTimeOffset TransferringCompletedAt { get; set; }

        public bool IsCompleted => TransferringCompletedAt != default(DateTimeOffset);

        public static HttpRequestRecordInfo CreateFromRecord(HttpRequestRecord record)
        {
            return new HttpRequestRecordInfo
            {
                Id = record.Id,
                ParentId = record.Id,
                IsHttps = record.IsHttps,
                Host = record.Host,
                QueryString = record.QueryString,
                Path = record.Path,
                Method = record.Method,
                ResponseStatusCode = record.ResponseStatusCode,
                RemoteIpAddress = record.RemoteIpAddress,
                RequestReceivedAt = record.RequestReceivedAt,
                TransferringCompletedAt = record.TransferringCompletedAt,
            };
        }
    }

    public class HttpRequestRecord : HttpRequestRecordInfo
    {
        public IDictionary<string, StringValues> RequestHeaders { get; set; } = default!;
        public IDictionary<string, StringValues>? RequestTrailers { get; set; }
        public IDictionary<string, StringValues>? ResponseHeaders { get; set; }
        public IDictionary<string, StringValues>? ResponseTrailers { get; set; }

        public ExceptionData? Exception { get; set; }
        public ITimelineScope Timeline { get; set; } = default!;

        public ITimelineScope Processing { get; set; } = default!;
        public ITimelineScope? Transferring { get; set; }
    }

    public class ExceptionData
    {
        public string ClassName { get; set; } = default!;
        public string FullName { get; set; } = default!;
        public string Message { get; set; } = default!;
        public string FullMessage { get; set; } = default!;
        public string StackTrace { get; set; } = default!;

        
      
        public ExceptionData()
        {
        }

        public ExceptionData(Exception ex)
        {
            if (ex == null) throw new ArgumentNullException(nameof(ex));

            ClassName = ex.GetType().Name;
            FullName = ex.GetType().FullName!;
            Message = ex.Message;
            FullMessage = ex.ToString();
            StackTrace = ex.StackTrace!;
        }
    }
}
