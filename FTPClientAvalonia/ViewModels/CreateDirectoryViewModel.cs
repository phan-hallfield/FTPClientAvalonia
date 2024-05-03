using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace FTPClientAvalonia.ViewModels;

public partial class CreateDirectoryViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _newDirectoryName;

    [ObservableProperty] private string _errorMessage;

    [RelayCommand]
    private async Task OkCommand()
    {
        
    }
    
    [RelayCommand]
    private async Task CancelCommand()
    {
        
    }
}