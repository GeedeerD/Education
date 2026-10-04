namespace FiweClient.Markdown;

/// <summary>Результат правки текста в поле ввода: новый текст и что выделить после неё.</summary>
public readonly record struct MarkdownEdit(string Text, int SelectionStart, int SelectionEnd);

/// <summary>
/// Операции форматирования для поля ввода сообщения (горячие клавиши и панель кнопок).
/// Чистые функции над строкой и выделением — сам TextBox меняется в code-behind ChatView.
/// </summary>
public static class MarkdownEditing
{
    public const string Bold = "**";
    public const string Italic = "*";
    public const string Strikethrough = "~~";
    public const string Code = "`";

    private const string LinkPlaceholderText = "текст";
    private const string LinkPlaceholderUrl = "https://";

    /// <summary>
    /// Оборачивает выделение маркером (**, *, ~~, `). Если выделение уже обёрнуто — снимает обёртку.
    /// Пробелы по краям выделения остаются снаружи: «** слово**» Markdown не распознаёт.
    /// Многострочное выделение с маркером кода превращается в блок кода.
    /// </summary>
    public static MarkdownEdit ToggleWrap(string text, int selectionStart, int selectionEnd, string marker)
    {
        var (start, end) = Normalize(text, selectionStart, selectionEnd);
        var m = marker.Length;

        // **[выделение]** — маркеры снаружи выделения
        if (IsWrappedBy(text, start - m, end, marker))
        {
            var unwrapped = text.Remove(end, m).Remove(start - m, m);
            return new MarkdownEdit(unwrapped, start - m, end - m);
        }

        // [**выделение**] — маркеры внутри выделения
        var selected = text[start..end];
        if (IsWrappedBy(selected, 0, selected.Length - m, marker))
        {
            var inner = selected[m..^m];
            return new MarkdownEdit(text[..start] + inner + text[end..], start, start + inner.Length);
        }

        if (marker == Code && selected.Contains('\n'))
            return WrapCodeBlock(text, start, end);

        // Пробелы по краям выделения выносим за маркеры
        while (start < end && char.IsWhiteSpace(text[start])) start++;
        while (end > start && char.IsWhiteSpace(text[end - 1])) end--;

        var result = text[..start] + marker + text[start..end] + marker + text[end..];
        return new MarkdownEdit(result, start + m, end + m);
    }

    /// <summary>
    /// [текст](https://) — выделенный текст становится текстом ссылки, а адрес выделяется,
    /// чтобы его сразу можно было вставить.
    /// </summary>
    public static MarkdownEdit InsertLink(string text, int selectionStart, int selectionEnd)
    {
        var (start, end) = Normalize(text, selectionStart, selectionEnd);
        var label = start == end ? LinkPlaceholderText : text[start..end];
        var link = $"[{label}]({LinkPlaceholderUrl})";

        var urlStart = start + label.Length + 3; // "[" + label + "]("
        return new MarkdownEdit(text[..start] + link + text[end..], urlStart, urlStart + LinkPlaceholderUrl.Length);
    }

    /// <summary>
    /// Добавляет префикс («> » — цитата, «- » — список) ко всем строкам, задетым выделением.
    /// Если он уже есть у всех этих строк — убирает.
    /// </summary>
    public static MarkdownEdit ToggleLinePrefix(string text, int selectionStart, int selectionEnd, string prefix)
    {
        var (start, end) = Normalize(text, selectionStart, selectionEnd);

        var blockStart = text.LastIndexOf('\n', Math.Max(start - 1, 0)) + 1;
        if (start == 0) blockStart = 0;
        var blockEnd = text.IndexOf('\n', end);
        if (blockEnd < 0) blockEnd = text.Length;

        var lines = text[blockStart..blockEnd].Split('\n');
        var remove = lines.All(l => l.StartsWith(prefix));
        var changed = string.Join('\n', lines.Select(l => remove ? l[prefix.Length..] : prefix + l));

        var result = text[..blockStart] + changed + text[blockEnd..];
        return new MarkdownEdit(result, blockStart, blockStart + changed.Length);
    }

    private static MarkdownEdit WrapCodeBlock(string text, int start, int end)
    {
        var before = start > 0 && text[start - 1] != '\n' ? "\n" : "";
        var after = end < text.Length && text[end] != '\n' ? "\n" : "";
        var opening = before + "```\n";
        var block = opening + text[start..end] + "\n```" + after;

        var innerStart = start + opening.Length;
        return new MarkdownEdit(text[..start] + block + text[end..], innerStart, innerStart + (end - start));
    }

    /// <summary>
    /// Стоят ли маркеры на позициях openAt и closeAt. Для односимвольного маркера проверяем,
    /// что рядом нет такого же символа: «*» внутри «**» — это жирный, а не курсив,
    /// и Ctrl+I по жирному тексту не должен его ломать.
    /// </summary>
    private static bool IsWrappedBy(string s, int openAt, int closeAt, string marker)
    {
        var m = marker.Length;
        if (openAt < 0 || closeAt < openAt + m || closeAt + m > s.Length)
            return false;
        if (!s.AsSpan(openAt, m).SequenceEqual(marker) || !s.AsSpan(closeAt, m).SequenceEqual(marker))
            return false;
        if (m > 1)
            return true;

        var c = marker[0];
        return !(openAt > 0 && s[openAt - 1] == c) &&
               !(openAt + 1 < closeAt && s[openAt + 1] == c) &&
               !(closeAt - 1 > openAt && s[closeAt - 1] == c) &&
               !(closeAt + 1 < s.Length && s[closeAt + 1] == c);
    }
    private static (int Start, int End) Normalize(string text, int a, int b)
    {
        var start = Math.Clamp(Math.Min(a, b), 0, text.Length);
        var end = Math.Clamp(Math.Max(a, b), 0, text.Length);
        return (start, end);
    }
}
