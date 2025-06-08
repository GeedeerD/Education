using Microsoft.Maui.Controls;

namespace FigureApp;

public partial class MainPage : ContentPage
{
    // Элементы управления
    Picker figurePicker;  // Список для выбора фигуры
    Label formulaLabel;   // Место для отображения формулы

    public MainPage()
    {
        Title = "Фигуры";  // Заголовок страницы

        // Создаём Picker для выбора фигуры
        figurePicker = new Picker
        {
            Title = "Выберите фигуру"
        };

        // Добавляем фигуры в список
        figurePicker.Items.Add("Квадрат");
        figurePicker.Items.Add("Круг");
        figurePicker.Items.Add("Куб");
        figurePicker.Items.Add("Сфера");
        figurePicker.Items.Add("Цилиндр");

        // Подключаем обработчик выбора фигуры
        figurePicker.SelectedIndexChanged += OnFigureSelected;

        // Создаём Label для отображения формулы
        formulaLabel = new Label
        {
            FontSize = 18,
            TextColor = Color.FromRgb(0, 0, 139), // Цвет текста
            HorizontalTextAlignment = TextAlignment.Center // Выравнивание по центру
        };

        // Создаем основное содержимое страницы
        Content = new StackLayout
        {
            Padding = 20,
            Spacing = 25,
            Children =
            {
                new Label
                {
                    Text = "Выберите фигуру",
                    FontSize = 24,
                    HorizontalOptions = LayoutOptions.Center // Выравнивание по центру
                },
                figurePicker, // Добавляем Picker
                formulaLabel  // Добавляем Label с формулой
            }
        };
    }

    // Обработчик выбора фигуры
    private void OnFigureSelected(object sender, EventArgs e)
    {
        // Получаем выбранную фигуру
        string selectedFigure = figurePicker.SelectedItem?.ToString();

        // В зависимости от выбора, обновляем текст с формулой
        switch (selectedFigure)
        {
            case "Квадрат":
                formulaLabel.Text = "Площадь квадрата: S = a²";
                break;
            case "Круг":
                formulaLabel.Text = "Площадь круга: S = π × r²";
                break;
            case "Куб":
                formulaLabel.Text = "Объём куба: V = a³";
                break;
            case "Сфера":
                formulaLabel.Text = "Объём сферы: V = 4/3 × π × r³";
                break;
            case "Цилиндр":
                formulaLabel.Text = "Объём цилиндра: V = π × r² × h";
                break;
            default:
                formulaLabel.Text = string.Empty; // Если фигура не выбрана
                break;
        }
    }
}
