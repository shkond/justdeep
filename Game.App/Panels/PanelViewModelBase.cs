using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Game.App.Panels;

/// <summary>
/// Base class for panel ViewModels. Automatically subscribes to UiEventBus
/// and routes events to the abstract OnEvent method.
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

    protected PanelViewModelBase(UiEventBus eventBus, IGameCommands commands)
    {
        Commands = commands;
        _subscription = eventBus.Subscribe(OnEvent);
    }

    public abstract void OnEvent(IUiEvent evt);

    public void Dispose()
    {
        _subscription.Dispose();
        GC.SuppressFinalize(this);
    }
}
