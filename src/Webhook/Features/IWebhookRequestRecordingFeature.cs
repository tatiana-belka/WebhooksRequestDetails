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
    public interface IWebhookRequestRecordingFeature
    {
        HttpRequestRecord Record { get; }
        DataCaptureStream? ResponseDataStream { get; set; }
    }
}
