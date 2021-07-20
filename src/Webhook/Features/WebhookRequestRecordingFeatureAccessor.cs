using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Webhook.Features
{
    public interface IRinRequestRecordingFeatureAccessor
    {
        IWebhookRequestRecordingFeature? Feature { get; }

        void SetValue(IWebhookRequestRecordingFeature? feature);
    }

    
    public class WebhookRequestRecordingFeatureAccessor : IWebhookRequestRecordingFeatureAccessor
    {
        private static readonly AsyncLocal<Holder> _current = new AsyncLocal<Holder>();

        public IWebhookRequestRecordingFeature? Feature => _current.Value?.Value;

        public void SetValue(IWebhookRequestRecordingFeature? feature)
        {
            if (_current.Value == null)
            {
                _current.Value = new Holder();
            }

            _current.Value.Value = feature;
        }

        private class Holder
        {
            public IWebhookRequestRecordingFeature? Value { get; set; }
        }
    }

    internal class NullWebhookRequestRecordingFeatureAccessor : IWebhookRequestRecordingFeatureAccessor
    {
        public static IWebhookRequestRecordingFeatureAccessor Instance { get; } = new NullRinRequestRecordingFeatureAccessor();

        private NullWebhookRequestRecordingFeatureAccessor()
        {}

        public IWebhookRequestRecordingFeature? Feature { get; } = default;

        public void SetValue(IWebhookRequestRecordingFeature? feature)
        {
        }
    }
}
