using System.Globalization;
using Avalonia.Data.Converters;

namespace FiweClient.Converters;

/// <summary>
/// Возвращает "active" если value == parameter, иначе пустую строку.
/// Используется для подсветки активной вкладки в боковой панели.
/// </summary>
public class EqualityConverter : IValueConverter
{
    public static readonly EqualityConverter Instance = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value?.ToString() == parameter?.ToString() ? "active" : "";

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
