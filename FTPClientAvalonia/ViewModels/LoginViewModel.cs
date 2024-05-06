using System;
using System.ComponentModel;
using System.Net.NetworkInformation;
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

        if (status != "Success")
        {
            ErrorMessage = status;
            return;
        }
        
        // Append server ip to ftp:// for use in FtpDirectoryViewModel
        _credentials.ServerIp = "ftp://" + _credentials.ServerIp;
        
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
    
    
    private async Task<string> ValidateConnection(string ftpServerIP, string username, string password)
    {
        string status = "";

        if (!IsIPAddressValidAndReachable(ftpServerIP.Split(':')[0]))
        {
            return "Invalid IP Address";
        }
        
        try
        {
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create("ftp://" + ftpServerIP);
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
            FtpWebResponse response = (FtpWebResponse)ex.Response;

            if (response == null)
            {
                status = "FTP Error: No response from server";
                return status;
            }
            
            Debug.WriteLine("FTP Error: {0} - {1}", response.StatusCode, response.StatusDescription);
            status = $"FTP Error: {response.StatusCode} - {response.StatusDescription}";
            response.Close();
            
            return status;  // Failed to connect or authenticate
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
            status = ex.Message;
            return status;
        }
    }
    
    public static bool IsIPAddressValidAndReachable(string ipAddress)
    {
        IPAddress address;
        bool isValid = IPAddress.TryParse(ipAddress, out address);
        if (!isValid)
        {
            return false; // IP address is not valid
        }

        using (Ping ping = new Ping())
        {
            try
            {
                PingReply reply = ping.Send(address, 1000); // Timeout is 1000 milliseconds
                return reply.Status == IPStatus.Success;
            }
            catch (Exception)
            {
                return false; // Ping failed or IP address is not reachable
            }
        }
    }
    
    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}