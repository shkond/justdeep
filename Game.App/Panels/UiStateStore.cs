using System;
using System.Collections.Generic;
using System.Linq;

namespace Game.App.Panels;

/// <summary>
/// Single-source-of-truth for UI state.
/// Uses record equality for automatic diff detection — subscribers
/// are only notified when the state actually changes.
/// </summary>
public class UiStateStore : IUiStateStore
{
    private UiState _current = UiState.Initial;
    private readonly List<Action<UiState>> _handlers = [];

    /// <inheritdoc />
    public UiState Current => _current;

    /// <inheritdoc />
    public IDisposable Subscribe(Action<UiState> handler)
    {
        _handlers.Add(handler);
        handler(_current); // Immediate notification with current state
        return new Subscription(this, handler);
    }

    /// <inheritdoc />
    public void Update(UiState newState)
    {
        if (_current == newState) return; // Record structural equality
        _current = newState;

        // Snapshot iteration allows subscribe/unsubscribe during notification
        foreach (var h in _handlers.ToArray())
        {
            h(_current);
        }
    }

    private sealed class Subscription(UiStateStore store, Action<UiState> handler) : IDisposable
    {
        private bool _disposed;

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            store._handlers.Remove(handler);
        }
    }
}
