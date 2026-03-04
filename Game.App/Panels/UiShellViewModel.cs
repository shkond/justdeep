using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Game.App.ViewModels;

namespace Game.App.Panels;

/// <summary>
/// UI Shell — manages which panels are displayed in which slots.
/// Three-zone layout: Nav (left), Workspace (center), Bottom (collapsible log).
/// </summary>
public partial class UiShellViewModel : ViewModelBase
{
    /// <summary>Panels displayed in the nav slot (left, fixed width).</summary>
    public ObservableCollection<IPanelViewModel> NavPanels { get; } = [];

    /// <summary>Panels displayed in the workspace slot (center, fills remaining space).</summary>
    public ObservableCollection<IPanelViewModel> WorkspacePanels { get; } = [];

    /// <summary>Panels displayed in the bottom slot (collapsible log area).</summary>
    public ObservableCollection<IPanelViewModel> BottomPanels { get; } = [];

    /// <summary>Whether the bottom panel area is expanded.</summary>
    [ObservableProperty]
    private bool _isBottomExpanded = true;

    /// <summary>Add a panel to a named slot ("Nav", "Workspace", or "Bottom").</summary>
    public void AddPanel(IPanelViewModel panel, string slot)
    {
        var target = GetSlot(slot);
        target.Add(panel);
    }

    /// <summary>Remove a panel by its ID from all slots.</summary>
    public void RemovePanel(string panelId)
    {
        RemoveFromSlot(NavPanels, panelId);
        RemoveFromSlot(WorkspacePanels, panelId);
        RemoveFromSlot(BottomPanels, panelId);
    }

    [RelayCommand]
    private void ToggleBottom()
    {
        IsBottomExpanded = !IsBottomExpanded;
    }

    private ObservableCollection<IPanelViewModel> GetSlot(string slot) =>
        slot switch
        {
            "Workspace" => WorkspacePanels,
            "Bottom" => BottomPanels,
            _ => NavPanels,
        };

    private static void RemoveFromSlot(ObservableCollection<IPanelViewModel> panels, string panelId)
    {
        for (int i = panels.Count - 1; i >= 0; i--)
        {
            if (panels[i].PanelId == panelId)
                panels.RemoveAt(i);
        }
    }
}
