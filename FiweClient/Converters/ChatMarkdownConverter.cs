using System.Globalization;
using Avalonia.Data.Converters;
using FiweClient.Markdown;

namespace FiweClient.Converters;

/// <summary>
/// Текст сообщения → Markdown для отрисовки в пузыре (см. <see cref="ChatMarkdown.Prepare"/>).
/// </summary>
public class ChatMarkdownConverter : IValueConverter
{
    public static readonly ChatMarkdownConverter Instance = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => ChatMarkdown.Prepare(value as string);

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
