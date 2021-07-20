using System;
using System.Collections.Generic;
using System.Text;

namespace Webhook.Hubs.Payloads
{
    public class WebhookServerInfoPayload
    {
        public string Version { get; }
        public DateTimeOffset BuildDate { get; }
        public IReadOnlyList<string> FeatureFlags { get; }

        public WebhookServerInfoPayload(string version, DateTimeOffset buildDate, IReadOnlyList<string> featureFlags)
        {
            Version = version;
            BuildDate = buildDate;
            FeatureFlags = featureFlags;
        }
    }
}
