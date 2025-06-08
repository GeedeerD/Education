using FigureApp; // <-- убедись, что пространство имён совпадает

namespace FigureApp;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
        MainPage = new MainPage(); // ← должно работать
    }

    private void InitializeComponent()
    {
        throw new NotImplementedException();
    }
}
