using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
using CommunityToolkit.Maui.Views;

namespace MobileFirst
{
    public partial class MainPage : ContentPage
    {
        int count = 0;

        public MainPage()
        {
            InitializeComponent();
        }

        private void OnImageTapped(object sender, EventArgs e)
        {
            // Показать popup
            this.ShowPopup(ImagePopup);
        }

        private void OnImageSelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is ImageSource selected)
            {
                SelectedImage.Source = selected;
                ImagePopup.Close(); // Закрыть popup
            }
        }
    }

}
