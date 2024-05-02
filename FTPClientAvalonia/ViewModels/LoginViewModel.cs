using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FTPClientAvalonia.Models;

namespace FTPClientAvalonia.ViewModels;

public partial class LoginViewModel : ViewModelBase, INotifyPropertyChanged
{
    private readonly FtpCredentials _credentials;
    
    public string FtpServerIP
    {
        get => _credentials.ServerIp;
        set { _credentials.ServerIp = value; OnPropertyChanged(); }
    }

    public string Username
    {
        get => _credentials.Username;
        set { _credentials.Username = value; OnPropertyChanged(); }
    }

    public string Password
    {
        get => _credentials.Password;
        set { _credentials.Password = value; OnPropertyChanged(); }
    }

    private string _loginFailed = "";
    public string ErrorMessage
    {
        get => _loginFailed;
        set { _loginFailed = value; OnPropertyChanged(); }
    }
    public LoginViewModel(FtpCredentials credentials)
    {
        _credentials = credentials;
    }
    
    
    
    /*
    [ObservableProperty] private string _errorMessage = "";
    [ObservableProperty] private string _serverIp = "";
    [ObservableProperty] private string _username = "";
    [ObservableProperty] private string _password = "";
    */
    
    [RelayCommand]
    private async Task Login()
    {
        Console.WriteLine("Login: " + FtpServerIP + " " + Username + " " + Password);
    }
    
    
    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}