namespace Game.App.Panels;

/// <summary>
/// Contract for all dockable panel ViewModels.
/// No longer carries event-handling responsibility.
/// </summary>
public interface IPanelViewModel
{
    /// <summary>Unique identifier for this panel type.</summary>
    string PanelId { get; }

    /// <summary>Display title for the panel header.</summary>
    string Title { get; }

    /// <summary>Whether this panel is currently visible.</summary>
    bool IsVisible { get; set; }
}
