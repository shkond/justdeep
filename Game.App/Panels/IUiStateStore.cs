using System;

namespace Game.App.Panels;

/// <summary>
/// Read/subscribe interface for UI state.
/// Mockable for panel VM unit tests.
/// </summary>
public interface IUiStateStore
{
    /// <summary>Current snapshot (never null).</summary>
    UiState Current { get; }

    /// <summary>
    /// Subscribe to state changes. The handler is called immediately
    /// with the current state, then again whenever state changes.
    /// Dispose the return value to unsubscribe.
    /// </summary>
    IDisposable Subscribe(Action<UiState> handler);

    /// <summary>
    /// Replace the current state. If the new state equals the current
    /// state (record structural equality), subscribers are NOT notified.
    /// </summary>
    void Update(UiState newState);
}
