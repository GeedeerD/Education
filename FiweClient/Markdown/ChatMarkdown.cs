using System.Text;
using System.Text.RegularExpressions;

namespace FiweClient.Markdown;

/// <summary>
/// Подготовка текста сообщения к отрисовке через Markdown.Avalonia.
/// Правки касаются только текста вне блоков и фрагментов кода:
///   — одиночный перенос строки становится переносом (как в мессенджерах, а не склейкой абзаца по CommonMark);
///   — картинки по внешним URL превращаются в ссылки: их загрузка раскрыла бы IP получателя
///     и прочитала бы содержимое E2EE-переписки мимо шифрования;
///   — «голые» http(s)-адреса становятся кликабельными ссылками.
/// </summary>
public static partial class ChatMarkdown
{
    public static string Prepare(string? text)
    {
        if (string.IsNullOrEmpty(text))
            return "";

        var lines = text.Replace("\r\n", "\n").Split('\n');
        var sb = new StringBuilder(text.Length + 16);
        string? openFence = null;

        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
            var trimmed = line.TrimStart();

            if (openFence is null && (trimmed.StartsWith("```") || trimmed.StartsWith("~~~")))
                openFence = trimmed[..3];
            else if (openFence is not null && trimmed.StartsWith(openFence))
                openFence = null;
            else if (openFence is null)
            {
                line = TransformOutsideCodeSpans(line);

                if (i + 1 < lines.Length && NeedsHardBreak(line, lines[i + 1]))
                    line += "  ";
            }

            sb.Append(line);
            if (i + 1 < lines.Length)
                sb.Append('\n');
        }

        return sb.ToString();
    }

    /// <summary>
    /// Две пробела в конце строки — жёсткий перенос в Markdown. Не трогаем пустые строки
    /// (это граница абзацев) и строки таблиц (лишние пробелы ломают разбор ячеек).
    /// </summary>
    private static bool NeedsHardBreak(string line, string nextLine) =>
        !string.IsNullOrWhiteSpace(line) &&
        !string.IsNullOrWhiteSpace(nextLine) &&
        !line.EndsWith("  ") &&
        !line.TrimStart().StartsWith('|') &&
        !nextLine.TrimStart().StartsWith('|');

    private static string TransformOutsideCodeSpans(string line)
    {
        if (!line.Contains('`'))
            return TransformText(line);

        // Чётные части — обычный текст, нечётные — содержимое `кода` вместе с обратными кавычками
        var sb = new StringBuilder(line.Length);
        var last = 0;
        foreach (Match m in CodeSpanRegex().Matches(line))
        {
            sb.Append(TransformText(line[last..m.Index]));
            sb.Append(m.Value);
            last = m.Index + m.Length;
        }
        sb.Append(TransformText(line[last..]));
        return sb.ToString();
    }

    private static string TransformText(string text)
    {
        text = ImageRegex().Replace(text, m =>
        {
            var alt = m.Groups["alt"].Value;
            return $"[🖼 {(alt.Length > 0 ? alt : "изображение")}]";
        });

        // Автоссылки вида <https://...> Markdown.Avalonia не поддерживает — оформляем обычной ссылкой
        return BareUrlRegex().Replace(text, m => $"[{m.Value}]({m.Value})");
    }

    [GeneratedRegex("(`+).+?\\1")]
    private static partial Regex CodeSpanRegex();

    // ![alt](url) и ![alt][ref] — оставляем только «[alt]», ссылочная часть остаётся на месте
    [GeneratedRegex(@"!\[(?<alt>[^\]]*)\](?=[\(\[])")]
    private static partial Regex ImageRegex();

    // Адрес, который ещё не оформлен как ссылка: не внутри (...), <...>, [...] и не часть другого слова
    [GeneratedRegex(@"(?<![\(<\[\w/])https?://[^\s<>()\[\]]+[^\s<>()\[\].,;:!?""'*_~]")]
    private static partial Regex BareUrlRegex();
}
