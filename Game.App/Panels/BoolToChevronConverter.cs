using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Game.App.Panels;

/// <summary>
/// Converts a boolean (IsBottomExpanded) to a chevron label for the toggle button.
/// true  → "▼ ログを隠す"
/// false → "▲ ログを表示"
/// </summary>
public class BoolToChevronConverter : IValueConverter
{
    public static readonly BoolToChevronConverter Instance = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is true ? "▼ ログを隠す" : "▲ ログを表示";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
