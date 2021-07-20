using System;

namespace Webhook.Core.Record
{
    public interface ITimelineEvent
    {
        
        string EventType { get; }

      
        string Name { get; set; }

      
        string Category { get; set; }

      
        string? Data { get; set; }

      
        DateTimeOffset Timestamp { get; set; }
    }
}
