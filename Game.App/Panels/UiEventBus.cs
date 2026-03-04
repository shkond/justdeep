using System;
using System.Collections.Generic;
using System.Linq;

namespace Game.App.Panels;

/// <summary>
/// Simple in-memory Pub/Sub for UI events.
/// Subscribe returns IDisposable for safe unsubscription (GC-safe).
/// </summary>
public class UiEventBus
{
    private readonly List<Action<IUiEvent>> _handlers = [];

    /// <summary>
    /// Subscribe to all events. Returns IDisposable — dispose to unsubscribe.
    /// </summary>
    public IDisposable Subscribe(Action<IUiEvent> handler)
    {
        _handlers.Add(handler);
        return new Subscription(this, handler);
    }

    /// <summary>Publish an event to all subscribers.</summary>
    public void Publish(IUiEvent evt)
    {
        // Iterate a snapshot to allow subscribe/unsubscribe during publish
        foreach (var h in _handlers.ToArray())
        {
            h(evt);
        }
    }

    private sealed class Subscription(UiEventBus bus, Action<IUiEvent> handler) : IDisposable
    {
        private bool _disposed;

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            bus._handlers.Remove(handler);
        }
    }
}
