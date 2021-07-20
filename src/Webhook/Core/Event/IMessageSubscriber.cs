using System.Threading.Tasks;

namespace Webhook.Core.Event
{
    public interface IMessageSubscriber<T>
    {
        Task Publish(T message);
    }
}
