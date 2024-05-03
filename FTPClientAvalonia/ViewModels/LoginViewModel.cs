using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FTPClientAvalonia.Models;
using FTPClientAvalonia.Views;

namespace FTPClientAvalonia.ViewModels;

public partial class LoginViewModel : ViewModelBase, INotifyPropertyChanged
{
    private readonly FtpCredentials _credentials;
    
    public string FtpServerIp
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
    
    [RelayCommand]
    private async Task Login()
    {
        string status = await ValidateConnection(_credentials.ServerIp, _credentials.Username, _credentials.Password);
        if (status == "Success")
        {
            // Navigate to FtpDirectoryView on successful Login
            Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                {
                    desktop.MainWindow.Content = new FtpDirectoryView
                    {
                        DataContext = new FtpDirectoryViewModel(_credentials)
                    };
                }
            });
        }
        else
        {
            ErrorMessage = status;
        }
    }
    
    
    private async Task<string> ValidateConnection(string ftpServerIP, string username, string password)
    {
        string status = "";
        
        try
        {
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(ftpServerIP);
            request.Method = WebRequestMethods.Ftp.PrintWorkingDirectory;  // Minimal FTP command
            request.Credentials = new NetworkCredential(username, password);

            using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
            {
                response.Close();
                return "Success";  // Successful connection and credential verification
            }
        }
        catch (WebException ex)
        {
            //TODO Bind to login failed error message
            FtpWebResponse response = (FtpWebResponse)ex.Response;
            if (response != null)
            {
                Debug.WriteLine("FTP Error: {0} - {1}", response.StatusCode, response.StatusDescription);
                status = $"FTP Error: {response.StatusCode} - {response.StatusDescription}";
                response.Close();
            }
            else
            {
                Debug.WriteLine("FTP Error: No response from server");
            }
            return status;  // Failed to connect or authenticate
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
            status = ex.Message;
            return status;
        }
    }
    
    
    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}