using CommunityToolkit.Maui.Views;

namespace MobileFirst;

public partial class ImagePickerPopup : Popup
{
    public ImagePickerPopup()
    {
        InitializeComponent();
    }

    private void OnImageSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is ImageSource selected)
        {
            Close(selected); // возвращаем выбранное изображение
        }
    }
}