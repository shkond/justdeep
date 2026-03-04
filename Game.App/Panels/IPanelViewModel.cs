namespace Game.App.Panels;

/// <summary>
/// Contract for all dockable panel ViewModels.
/// </summary>
public interface IPanelViewModel
{
    /// <summary>Unique identifier for this panel type.</summary>
    string PanelId { get; }

    /// <summary>Display title for the panel header.</summary>
    string Title { get; }

    /// <summary>Whether this panel is currently visible.</summary>
    bool IsVisible { get; set; }

    /// <summary>Handle a UI event (differential, not per-tick).</summary>
    void OnEvent(IUiEvent evt);
}
