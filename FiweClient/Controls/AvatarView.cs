using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;

namespace FiweClient.Controls;

/// <summary>
/// Круглый аватар: картинка (если задан <see cref="Source"/>) или инициалы на цветном фоне.
/// Цвет фона стабилен для одного и того же имени — собеседников легко различать в списке.
///
/// Задел под аватары пользователей (issue #42 «User Avatars»): сейчас <see cref="Source"/> нигде
/// не заполняется — когда появится загрузка картинок профиля, достаточно передать сюда Bitmap.
/// </summary>
public class AvatarView : Border
{
    public static readonly StyledProperty<string?> DisplayNameProperty =
        AvaloniaProperty.Register<AvatarView, string?>(nameof(DisplayName));

    public static readonly StyledProperty<Bitmap?> SourceProperty =
        AvaloniaProperty.Register<AvatarView, Bitmap?>(nameof(Source));

    public static readonly StyledProperty<double> SizeProperty =
        AvaloniaProperty.Register<AvatarView, double>(nameof(Size), 44);

    // Спокойные приглушённые цвета, хорошо читаемые с белым текстом
    private static readonly Color[] Palette =
    [
        Color.Parse("#4F7A74"), Color.Parse("#8D6E63"), Color.Parse("#5E7C9B"),
        Color.Parse("#A0694B"), Color.Parse("#7A6A9A"), Color.Parse("#6B8752"),
        Color.Parse("#9A5B6B"), Color.Parse("#4E7E95"), Color.Parse("#8C7A45"),
    ];

    private readonly TextBlock _initials = new()
    {
        Foreground = Brushes.White,
        FontWeight = FontWeight.SemiBold,
        HorizontalAlignment = HorizontalAlignment.Center,
        VerticalAlignment = VerticalAlignment.Center,
    };

    private readonly Image _image = new() { Stretch = Stretch.UniformToFill, IsVisible = false };

    /// <summary>Имя, из которого берутся инициалы и цвет.</summary>
    public string? DisplayName
    {
        get => GetValue(DisplayNameProperty);
        set => SetValue(DisplayNameProperty, value);
    }

    /// <summary>Картинка аватара. Пока не задана — показываются инициалы.</summary>
    public Bitmap? Source
    {
        get => GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }

    /// <summary>Диаметр в пикселях.</summary>
    public double Size
    {
        get => GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    public AvatarView()
    {
        ClipToBounds = true;
        Child = new Panel { Children = { _initials, _image } };
        ApplySize();
        ApplyName();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == DisplayNameProperty)
            ApplyName();
        else if (change.Property == SizeProperty)
            ApplySize();
        else if (change.Property == SourceProperty)
        {
            _image.Source = Source;
            _image.IsVisible = Source is not null;
            _initials.IsVisible = Source is null;
        }
    }

    private void ApplySize()
    {
        Width = Height = Size;
        CornerRadius = new CornerRadius(Size / 2);
        _initials.FontSize = Math.Max(10, Size * 0.38);
    }

    private void ApplyName()
    {
        var name = DisplayName?.Trim() ?? "";
        _initials.Text = GetInitials(name);
        Background = new SolidColorBrush(Palette[StableHash(name) % Palette.Length]);
    }

    public static string GetInitials(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return "?";

        var parts = name.Split([' ', '_', '-', '.'], StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length >= 2)
            return $"{char.ToUpperInvariant(parts[0][0])}{char.ToUpperInvariant(parts[1][0])}";

        return char.ToUpperInvariant(name[0]).ToString();
    }

    // string.GetHashCode() меняется между запусками — нужен стабильный хеш, чтобы цвет не «прыгал»
    private static int StableHash(string s)
    {
        unchecked
        {
            var h = 17;
            foreach (var c in s) h = h * 31 + c;
            return h & 0x7FFFFFFF;
        }
    }
}
