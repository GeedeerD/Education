using CommunityToolkit.Mvvm.ComponentModel;
using FiweClient.Views;
using Microsoft.Extensions.DependencyInjection;

namespace FiweClient.Services.Navigation;

/// <summary>
/// Управляет навигацией между экранами.
/// Меняет CurrentView в MainWindowViewModel,
/// Avalonia автоматически рендерит нужный View через DataTemplates.
/// </summary>
public interface INavigationService
{
    void SetHost(MainWindow window);
    void SetHost(MainView view);
    void NavigateTo<TViewModel>() where TViewModel : ObservableObject;
    void NavigateTo(ObservableObject viewModel);
}

public class NavigationService : INavigationService
{
    private readonly IServiceProvider _services;
    private MainWindowViewModel? _mainVm;

    public NavigationService(IServiceProvider services)
    {
        _services = services;
    }

    public void SetHost(MainWindow window)
    {
        _mainVm = window.DataContext as MainWindowViewModel;
    }
    public void SetHost(MainView view) => _mainVm = view.DataContext as MainWindowViewModel;

    /// <summary>
    /// Создаёт ViewModel через DI и переходит к нему.
    /// Пример: nav.NavigateTo&lt;LoginViewModel&gt;()
    /// </summary>
    public void NavigateTo<TViewModel>() where TViewModel : ObservableObject
    {
        var vm = _services.GetRequiredService<TViewModel>();
        NavigateTo(vm);
    }

    public void NavigateTo(ObservableObject viewModel)
    {
        if (_mainVm is null)
            throw new InvalidOperationException("NavigationService: host not set");

        _mainVm.CurrentView = viewModel;
    }
}
