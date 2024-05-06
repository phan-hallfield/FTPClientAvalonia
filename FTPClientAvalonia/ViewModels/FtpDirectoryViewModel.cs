using System.Collections.ObjectModel;
using System.Net.Mime;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.Input;
using FTPClientAvalonia.Views;
using Avalonia.Platform.Storage;
using Avalonia.Platform.Storage.FileIO;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Linq;

namespace FTPClientAvalonia.ViewModels;

public partial class FtpDirectoryViewModel : ViewModelBase
{
    private readonly FtpCredentials _credentials;
    private TaskCompletionSource<bool> _confirmationTaskSource;
    
    [ObservableProperty] private FtpItem _selectedItem;
    [ObservableProperty] private string _statusMessage;
    [ObservableProperty] private string _statusColor;
    [ObservableProperty] private string _title;
    [ObservableProperty] private string _newDirectoryName;
    [ObservableProperty] private bool _isDirectoryInputVisible;
    [ObservableProperty] private bool _isGroupAVisible = true;
    [ObservableProperty] private bool _isGroupBVisible;
    
    public ObservableCollection<FtpItem> Items { get; set; } = new ObservableCollection<FtpItem>();

    [RelayCommand]
    private async Task Download()
    {
        // Don't do anything if no item is selected
        if (!ValidateItemSelection())
            return;

        if (SelectedItem.IsDirectory)
        {
            StatusMessage = "Cannot download a directory";
            StatusColor = "#c93e34";
            return;
        }
        
        // Open folder picker dialog
        string folderPath = await this.OpenFolderDialogAsync("Select Download Folder");
        
        // Null folder path returned if user cancels on prompt
        if (string.IsNullOrEmpty(folderPath))
            return;
        
        StatusMessage = await FtpMethods.DownloadFile(_credentials, SelectedItem.Name, Path.Combine(folderPath, SelectedItem.Name));
        if (StatusMessage.Contains("Success"))
        {
            StatusMessage = $"Downloaded {SelectedItem.Name} to {folderPath}\n {StatusMessage}";
            StatusColor = "Green";
            Items.Clear();
            InitializeDataAsync();
        }
        else
        {
            StatusColor = "Red";
        }
    }
    
    
    
    [RelayCommand]
    private async Task Upload()
    {
        // TODO: Add upload
        IStorageFile uploadFile = await this.OpenFileDialogAsync("Select File to Upload");

        if (uploadFile == null)
            return;
        
        StatusMessage = await FtpMethods.UploadFile(_credentials, uploadFile.TryGetLocalPath(), uploadFile.Name);
        if (StatusMessage.Contains("Success"))
        {
            StatusMessage = $"{uploadFile.Name} uploaded\n{StatusMessage}";
            StatusColor = "Green";
            Items.Clear();
            InitializeDataAsync();
        }
        else
            StatusColor = "Red";
    }
    
    [RelayCommand]
    private async Task Delete()
    {
        // Don't do anything if no item is selected
        if (!ValidateItemSelection())
            return;

        IsGroupAVisible = false;
        IsGroupBVisible = true;
        
        string itemType = SelectedItem.IsDirectory ? "directory" : "file";
        string itemName = SelectedItem.Name;
        StatusMessage = $"Are you sure you want to delete the {itemType} '{itemName}'?";
        StatusColor = "Red";
        
        // Wait for user confirmation
        _confirmationTaskSource = new TaskCompletionSource<bool>();

        bool confirmed = await _confirmationTaskSource.Task;

        if (!confirmed)
        {
            StatusMessage = "";
            StatusColor = "Transparent";
            IsGroupAVisible = true;
            IsGroupBVisible = false;
            return;
        }
        
        if (_selectedItem.IsDirectory)
        {
            StatusMessage = await FtpMethods.DeleteDirectory(_credentials, itemName);
            if (StatusMessage.Contains("Success"))
            {
                Items.Clear();
                InitializeDataAsync();
                StatusColor = "Green";
            }
            else
                StatusColor = "Red";
        }
        else
        {
            StatusMessage = await FtpMethods.DeleteFile(_credentials, itemName);
            if (StatusMessage.Contains("Success"))
            {
                Items.Clear();
                InitializeDataAsync();
                StatusColor = "Green";
            }
            else
                StatusColor = "Red";
        }
        IsGroupAVisible = true;
        IsGroupBVisible = false;
       
        
    }
    
    [RelayCommand]
    private async Task CreateDirectory()
    {
        IsGroupAVisible = false;
        IsGroupBVisible = true;
        IsDirectoryInputVisible = true;
        
        StatusMessage = "Enter new directory name:";
        StatusColor = "White";
        
        // Wait for user confirmation
        _confirmationTaskSource = new TaskCompletionSource<bool>();

        bool confirmed = await _confirmationTaskSource.Task;

        // Reset buttons if user cancels
        if (!confirmed)
        {
            StatusMessage = "";
            StatusColor = "Transparent";
            IsGroupAVisible = true;
            IsGroupBVisible = false;
            IsDirectoryInputVisible = false;
            return;
        }
        
        string directoryName = NewDirectoryName;
        
        // Check if directory name is empty
        if (string.IsNullOrEmpty(directoryName))
        {
            IsGroupAVisible = true;
            IsGroupBVisible = false;
            IsDirectoryInputVisible = false;
            StatusMessage = "Can't create directory with empty name";
            StatusColor = "#c93e34";
            return;
        }
        
        // Attempt directory creation and displsy status message
        StatusMessage = await FtpMethods.CreateDirectory(_credentials, directoryName);
        if (StatusMessage.Contains("Success"))
        {
            Items.Clear();
            InitializeDataAsync();
            StatusColor = "Green";
        }
        else
        {
            StatusColor = "#c93e34";
        }
        
        // Reset buttons
        IsGroupAVisible = true;
        IsGroupBVisible = false;
        IsDirectoryInputVisible = false;
        
    }
    
    // Constructor
    public FtpDirectoryViewModel(FtpCredentials credentials)
    {
        _credentials = credentials;
        InitializeDataAsync();
    }
    
    private async void InitializeDataAsync()
    {
        Title = _credentials.ServerIp.Substring(6);
        try
        {
            await ListDirectoryDetails();
        }
        catch (Exception ex)
        {
            StatusMessage = "Error loading directory contents: " + ex.Message;
            StatusColor = "#c93e34";
        }
    }
    
    private async Task ListDirectoryDetails()
    {
        Items.Clear();
        var directoryDetails = await FtpMethods.GetDirectoryDetails(_credentials);
        foreach (var detail in directoryDetails)
        {
            FtpItem item = FtpItemConverter.ConvertFromDirectoryDetail(detail);
            Items.Add(item);
        }
        
        // Sort by name
        var sortedItems = Items.OrderBy(item => item.Name).ToList();
        Items.Clear();
        foreach (var item in sortedItems)
        {
            Items.Add(item);
        }
    }
    
    // Checks whether an item is selected
    private bool ValidateItemSelection()
    {
        if (_selectedItem == null)
        {
            StatusMessage = "Please select an item";
            StatusColor = "#c93e34";
            return false;
        }
        
        return true;
    }
    
    [RelayCommand]
    private void Confirm()
    {
        _confirmationTaskSource?.SetResult(true);
    }

    [RelayCommand]
    private void Cancel()
    {
        _confirmationTaskSource?.SetResult(false);
    }
}