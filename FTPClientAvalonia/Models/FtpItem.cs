namespace FTPClientAvalonia.Models;

public class FtpItem
{
    public string Name { get; set; }
    public bool IsDirectory { get; set; }
    public string IconPath => IsDirectory ?
        "/Assets/folder_icon.png" :
        "/Assets/file_icon.png";
    public long Size { get; set; }
    
    public string[] LastModified { get; set; }
}