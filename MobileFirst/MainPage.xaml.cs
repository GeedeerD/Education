using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
using CommunityToolkit.Maui.Views;
using MobileFirst;

namespace MobileFirst
{
    public partial class MainPage : ContentPage
    {
        int count = 0;

        public MainPage()
        {
            InitializeComponent();
        }

        private void OnShapeSelected(object sender, EventArgs e)
        {
            string selectedShape = ShapePicker.SelectedItem as string;

            switch (selectedShape)
            {
                case "Треугольник":
                    ShapeImage.Source = "triangle.png";
                    break;
                case "Круг":
                    ShapeImage.Source = "circle.png";
                    break;
                case "Квадрат":
                    ShapeImage.Source = "square.png";
                    break;
                case "Полигон":
                    ShapeImage.Source = "polygon.png";
                    break;
                default:
                    ShapeImage.Source = null;
                    break;
            }
        }
    }

}
