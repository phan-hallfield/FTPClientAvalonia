using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using FTPClientAvalonia.Models;
using FTPClientAvalonia.ViewModels;
using FTPClientAvalonia.Views;

namespace FTPClientAvalonia
{
    public partial class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                FtpCredentials creds = new FtpCredentials();
                #if DEBUG
                creds.ServerIp = "ftp://192.168.1.23:21";
                creds.Username = "tony";
                creds.Password = "1234";
                #endif
                // Line below is needed to remove Avalonia data validation.
                // Without this line you will get duplicate validations from both Avalonia and CT
                BindingPlugins.DataValidators.RemoveAt(0);
                desktop.MainWindow = new MainWindow
                {
                    
                    DataContext = new LoginViewModel(creds),
                    //DataContext = new FtpDirectoryViewModel(creds),
                };
            }
            
            

            base.OnFrameworkInitializationCompleted();
        }
    }
}