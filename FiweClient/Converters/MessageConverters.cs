using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;

using Application = Avalonia.Application;

namespace FiweClient.Converters;

/// <summary>
/// true (моё сообщение) → Right, false → Left
/// </summary>
public class BoolToAlignmentConverter : IValueConverter
{
    public static readonly BoolToAlignmentConverter Instance = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is true ? HorizontalAlignment.Right : HorizontalAlignment.Left;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

/// <summary>
/// Цвет текста: моё сообщение → всегда белый, чужое → системный
/// </summary>
public class BubbleTextColorConverter : IValueConverter
{
    public static readonly BubbleTextColorConverter Instance = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        // Чужое сообщение — цвет не задаём: текст наследует цвет темы и сам переключается между светлой и тёмной.
        // (Раньше здесь искался ресурс SystemBaseHighColor — это Color, а не IBrush, поэтому всегда получался чёрный.)
        => value is true ? Brushes.White : AvaloniaProperty.UnsetValue;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
