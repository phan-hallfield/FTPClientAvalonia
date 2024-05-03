namespace FTPClientAvalonia.Services;

public static class FtpItemConverter
{
    public static FtpItem ConvertFromDirectoryDetail(string directoryDetail)
    {
        if (string.IsNullOrEmpty(directoryDetail))
            throw new ArgumentException("Directory detail cannot be null or empty.");

        // Split the string by whitespace and take parts
        string[] parts = directoryDetail.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        
        if (parts.Length < 9) // Less than the minimum parts needed to parse correctly
            throw new FormatException("Directory detail is not in the expected format.");

        // Check if the entry is a directory
        bool isDirectory = parts[0][0] == 'd';

        // Extract the file size
        long size = long.Parse(parts[4]);
        
        // Extract the last modified date
        string month = parts[5];
        string day = parts[6];
        string time = parts[7];
        
        // Extract the name, which is the last part of the details and might contain spaces
        string name = string.Join(" ", parts, 8, parts.Length - 8);
        
        // Add a trailing slash if the entry is a directory
        if (isDirectory)
            name = name + "/";

        return new FtpItem
        {
            Name = name,
            IsDirectory = isDirectory,
            Size = size,
            LastModified = new[] { month, day, time }
        };
    }
}