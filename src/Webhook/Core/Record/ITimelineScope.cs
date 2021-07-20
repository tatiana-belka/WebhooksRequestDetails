using System;
using System.Collections.Generic;

namespace Webhook.Core.Record
{
    public interface ITimelineScope : ITimelineEvent, IDisposable
    {
        
        TimeSpan Duration { get; set; }

        
        IReadOnlyCollection<ITimelineEvent> Children { get; }

        
        void Complete();
    }

    public interface ITimelineScopeCreatable : ITimelineScope
    {
        
        ITimelineScope Create(string name, string category, string? data);
    }
}
