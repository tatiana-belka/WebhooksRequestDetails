using System.Threading.Tasks;

namespace Webhook.Channel
{
    public abstract class HubClientBase<THub, TClient>
    {
        private WebhookChannel _channel;
        public HubClientBase(WebhookChannel channel)
        {
            _channel = channel;
        }

        protected Task InvokeAsync(string methodName, object[] args)
        {
            return _channel.InvokeAsync<THub>(methodName, args);
        }
    }

}
