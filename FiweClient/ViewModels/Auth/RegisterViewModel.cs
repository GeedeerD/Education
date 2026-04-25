using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FiweClient.Services.Api;
using FiweClient.Services.Navigation;

namespace FiweClient.ViewModels.Auth;

public partial class RegisterViewModel : ObservableObject
{
    private readonly IAuthApiService _authApi;
    private readonly INavigationService _navigation;

    [ObservableProperty] private string _username = "";
    [ObservableProperty] private string _password = "";
    [ObservableProperty] private string _confirmPassword = "";
    [ObservableProperty] private string? _errorMessage;
    [ObservableProperty] private bool _isBusy;

    public RegisterViewModel(IAuthApiService authApi, INavigationService navigation)
    {
        _authApi = authApi;
        _navigation = navigation;
    }

    [RelayCommand]
    private async Task RegisterAsync()
    {
        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Заполните все поля";
            return;
        }

        if (Password != ConfirmPassword)
        {
            ErrorMessage = "Пароли не совпадают";
            return;
        }

        IsBusy = true;
        ErrorMessage = null;

        try
        {
            await _authApi.RegisterAsync(Username, Password);
            // После регистрации — на логин
            _navigation.NavigateTo<LoginViewModel>();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Ошибка регистрации: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void GoToLogin()
        => _navigation.NavigateTo<LoginViewModel>();
}
