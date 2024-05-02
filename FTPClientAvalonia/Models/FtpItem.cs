namespace FTPClientAvalonia.Models;

public class FtpItem
{
    public string Name { get; set; }
    public bool IsDirectory { get; set; }
    public string IconPath => IsDirectory ? "folder_icon.png" : "file_icon.png";
    public long Size { get; set; }
}