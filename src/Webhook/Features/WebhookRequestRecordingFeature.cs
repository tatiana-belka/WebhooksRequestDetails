using Webhook.Core;
using Webhook.Core.Record;
using Webhook.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Webhook.Features
{
    public class WebhookRequestRecordingFeature : IWebhookRequestRecordingFeature
    {
        public HttpRequestRecord Record { get; }
        public DataCaptureStream? ResponseDataStream { get; set; }

        public WebhookRequestRecordingFeature(HttpRequestRecord record)
        {
            Record = record;
        }
    }
}
