using System;
using System.Diagnostics.CodeAnalysis;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Game.App.ViewModels;
using Game.App.Panels;

namespace Game.App;

/// <summary>
/// Given a view model, returns the corresponding view if possible.
/// Supports both traditional ViewModels (Game.App.ViewModels → Game.App.Views)
/// and Panel ViewModels (Game.App.Panels.XxxViewModel → Game.App.Panels.Views.Xxx).
/// </summary>
[RequiresUnreferencedCode(
    "Default implementation of ViewLocator involves reflection which may be trimmed away.",
    Url = "https://docs.avaloniaui.net/docs/concepts/view-locator")]
public class ViewLocator : IDataTemplate
{
    public Control? Build(object? param)
    {
        if (param is null)
            return null;

        var fullName = param.GetType().FullName!;
        string? viewName = null;

        // Panel VMs: Game.App.Panels.XxxPanelViewModel → Game.App.Panels.Views.XxxPanel
        if (fullName.StartsWith("Game.App.Panels.", StringComparison.Ordinal)
            && fullName.EndsWith("ViewModel", StringComparison.Ordinal))
        {
            // e.g. "Game.App.Panels.PlayerInfoPanelViewModel"
            //    → "Game.App.Panels.Views.PlayerInfoPanel"
            var className = param.GetType().Name; // "PlayerInfoPanelViewModel"
            var viewClassName = className.Replace("ViewModel", "", StringComparison.Ordinal);
            var ns = "Game.App.Panels.Views";
            viewName = $"{ns}.{viewClassName}";
        }
        // Traditional ViewModels: Game.App.ViewModels.XxxViewModel → Game.App.Views.Xxx
        else if (fullName.Contains("ViewModel", StringComparison.Ordinal))
        {
            viewName = fullName.Replace("ViewModel", "View", StringComparison.Ordinal);
        }

        if (viewName != null)
        {
            var type = Type.GetType(viewName);
            if (type != null)
            {
                return (Control)Activator.CreateInstance(type)!;
            }
        }
        
        return new TextBlock { Text = "Not Found: " + (viewName ?? fullName) };
    }

    public bool Match(object? data)
    {
        return data is ViewModelBase || data is IPanelViewModel;
    }
}
