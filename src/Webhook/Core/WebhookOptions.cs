using Webhook.Core.Record;
using Webhook.Core.Storage;
using System;
using System.Collections.Generic;
using System.Text;

namespace Webhook.Core
{
    public class WebhookOptions
    {
        public RequestRecorderOptions RequestRecorder { get; set; } = new RequestRecorderOptions();
        public InspectorOptions Inspector { get; set; } = new InspectorOptions();
    }
}
