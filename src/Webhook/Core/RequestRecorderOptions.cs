using Microsoft.AspNetCore.Http;
using Webhook.Core.Record;
using Webhook.Core.Storage;
using System;
using System.Collections.Generic;

namespace Webhook.Core
{
    public class RequestRecorderOptions
    {
        public List<Func<HttpRequest, bool>> Excludes { get; set; } = new List<Func<HttpRequest, bool>>();

        public int RetentionMaxRequests { get; set; } = 100;

        public bool EnableBodyCapturing { get; set; } = true;

        public bool AllowRunningOnProduction { get; set; } = false;
    }
}
