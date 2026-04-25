using CommunityToolkit.Mvvm.ComponentModel;

namespace FiweClient;

/// <summary>
/// ViewModel главного окна.
/// Содержит CurrentView — текущий отображаемый экран.
/// MainWindow биндится на него и через DataTemplates показывает нужный View.
/// </summary>
public partial class MainWindowViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableObject? _currentView;
}
