using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Game.App.ViewModels;

namespace Game.App.Panels;

/// <summary>
/// UI Shell — manages which panels are displayed in which slots.
/// The only layout concern; panels themselves have no knowledge of layout.
/// </summary>
public partial class UiShellViewModel : ViewModelBase
{
    public UiEventBus EventBus { get; }

    /// <summary>Panels displayed in the left slot.</summary>
    public ObservableCollection<IPanelViewModel> LeftPanels { get; } = [];

    /// <summary>Panels displayed in the right slot.</summary>
    public ObservableCollection<IPanelViewModel> RightPanels { get; } = [];

    public UiShellViewModel(UiEventBus eventBus)
    {
        EventBus = eventBus;
    }

    /// <summary>Add a panel to a named slot ("Left" or "Right").</summary>
    public void AddPanel(IPanelViewModel panel, string slot)
    {
        var target = GetSlot(slot);
        target.Add(panel);
    }

    /// <summary>Remove a panel by its ID from all slots.</summary>
    public void RemovePanel(string panelId)
    {
        RemoveFromSlot(LeftPanels, panelId);
        RemoveFromSlot(RightPanels, panelId);
    }

    private ObservableCollection<IPanelViewModel> GetSlot(string slot) =>
        slot switch
        {
            "Right" => RightPanels,
            _ => LeftPanels,
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
