using System.Globalization;
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
/// Цвет пузыря: моё сообщение → акцентный синий, чужое → зависит от темы
/// </summary>
//public class BubbleColorConverter : IMultiValueConverter
//{
//    public static readonly BubbleColorConverter Instance = new();

//    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
//    {
//        if (values[0] is not bool isMine) return Brushes.Gray;

//        if (isMine)
//            return new SolidColorBrush(Color.Parse("#5B6EF5"));

//        // Чужое сообщение — цвет зависит от темы
//        var isDark = Application.Current?.ActualThemeVariant == ThemeVariant.Dark;
//        return isDark
//            ? new SolidColorBrush(Color.Parse("#2E2E3E"))
//            : new SolidColorBrush(Color.Parse("#EBEBF5"));
//    }
//}
public class BubbleColorConverter : IValueConverter
{
    public static readonly BubbleColorConverter Instance = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not bool isMine) return Brushes.Gray;

        if (isMine)
            return new SolidColorBrush(Color.Parse("#5B6EF5"));

        // Определяем тему через Application напрямую из C#
        var isDark = Application.Current?.ActualThemeVariant == ThemeVariant.Dark;
        return isDark
            ? new SolidColorBrush(Color.Parse("#2E2E3E"))
            : new SolidColorBrush(Color.Parse("#EBEBF5"));
    }

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
        => value is true ? Brushes.White : Application.Current?.FindResource("SystemBaseHighColor") as IBrush ?? Brushes.Black;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
