using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Game.App.Panels;

/// <summary>
/// Base class for panel ViewModels. Automatically subscribes to UiStateStore
/// and routes state changes to the abstract OnStateChanged method.
/// Dispose to unsubscribe.
/// </summary>
public abstract partial class PanelViewModelBase : ObservableObject, IPanelViewModel, IDisposable
{
    private readonly IDisposable _subscription;

    public abstract string PanelId { get; }
    public abstract string Title { get; }

    [ObservableProperty]
    private bool _isVisible = true;

    protected IGameCommands Commands { get; }

    protected PanelViewModelBase(IUiStateStore store, IGameCommands commands)
    {
        Commands = commands;
        _subscription = store.Subscribe(OnStateChanged);
    }

    /// <summary>
    /// Called when UI state changes. Implementations update their
    /// observable properties and visibility from the new state.
    /// </summary>
    protected abstract void OnStateChanged(UiState state);

    public void Dispose()
    {
        _subscription.Dispose();
        GC.SuppressFinalize(this);
    }
}
