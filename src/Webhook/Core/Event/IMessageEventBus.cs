using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Webhook.Core.Event
{
    public interface IMessageEventBus<T> : IDisposable
    {
        void Subscribe(IEnumerable<IMessageSubscriber<T>> subscribers);
        ValueTask PostAsync(T value);
    }
}
