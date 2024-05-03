namespace FTPClientAvalonia.Services;

public class FtpMethods
{
    public static async Task<List<string>> GetDirectoryDetails(FtpCredentials credentials)
    {
        List<string> directoryDetails = new List<string>();
        FtpWebRequest request = (FtpWebRequest)WebRequest.Create(credentials.ServerIp);
        request.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
        request.Credentials = new NetworkCredential(credentials.Username, credentials.Password);

        Debug.WriteLine("checking credentials");
        Debug.WriteLine("Credentials: " + credentials.ServerIp + " " + credentials.Username + " " + credentials.Password);
        using (FtpWebResponse response = (FtpWebResponse)await request.GetResponseAsync())
        {
            using (Stream responseStream = response.GetResponseStream())
            {
                using (StreamReader reader = new StreamReader(responseStream))
                {
                    string line;
                    while ((line = await reader.ReadLineAsync()) != null)
                    {
                        Debug.WriteLine(line);
                        directoryDetails.Add(line);
                    }
                }
            }
        }
        return directoryDetails;
    }

    public static async Task<string> DownloadFile(FtpCredentials credentials, string remoteFile, string localFile)
    {
        // Construct the FTP URI
        string uri = $"{credentials.ServerIp}/{remoteFile}";
        string status = "";

        FtpWebRequest request = (FtpWebRequest)WebRequest.Create(uri);
        request.Method = WebRequestMethods.Ftp.DownloadFile;
        request.Credentials = new NetworkCredential(credentials.Username, credentials.Password);

        Debug.WriteLine("Attempting download...");
        Debug.WriteLine($"URI: {uri}");
        Debug.WriteLine($"Credentials: {credentials.Username} {credentials.Password}");

        try
        {
            // Get the response from the FTP server
            using (FtpWebResponse response = (FtpWebResponse)await request.GetResponseAsync())
            {
                // Open a stream to read the file from the server
                using (Stream responseStream = response.GetResponseStream())
                {
                    // Create a local file stream to write the downloaded file
                    using (FileStream fileStream = new FileStream(localFile, FileMode.Create))
                    {
                        // Copy the response stream to the local file stream
                        await responseStream.CopyToAsync(fileStream);
                    }
                }
                status = "Success: " + response.StatusDescription;
                Debug.WriteLine($"Download Complete, status {response.StatusDescription}");
                return status;
            }
        }
        catch (Exception ex)
        {
            status = "Error: " + ex.Message;
            Debug.WriteLine($"An error occurred during the FTP download: {ex.Message}");
            return status;
        }
    }

    public static async Task<string> UploadFile(FtpCredentials credentials, string localFile, string remoteFile)
    {
        string status = "";

        // Construct the FTP URI
        string uri = $"{credentials.ServerIp}/{remoteFile}";

        // Create an FTPWebRequest object for the upload
        FtpWebRequest request = (FtpWebRequest)WebRequest.Create(uri);
        request.Method = WebRequestMethods.Ftp.UploadFile;
        request.Credentials = new NetworkCredential(credentials.Username, credentials.Password);

        try
        {
            // Open a file stream to read the file for upload
            using (FileStream fileStream = new FileStream(localFile, FileMode.Open, FileAccess.Read))
            {
                // Get the request stream
                using (Stream requestStream = await request.GetRequestStreamAsync())
                {
                    // Copy the file contents to the request stream
                    await fileStream.CopyToAsync(requestStream);
                }
            }

            // Complete the request and check the response
            using (FtpWebResponse response = (FtpWebResponse)await request.GetResponseAsync())
            {
                Debug.WriteLine($"Upload Complete, status {response.StatusDescription}");
                status = "Success: " + response.StatusDescription;
                return status;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"An error occurred during the FTP upload: {ex.Message}");
            status = "Error: " + ex.Message;
            return status;
        }
    }

    public static async Task<string> DeleteFile(FtpCredentials credentials, string remoteFile)
    {
        string status = "";

        // Construct the FTP URI
        string uri = $"{credentials.ServerIp}/{remoteFile}";

        // Create an FTPWebRequest object for deleting a file
        FtpWebRequest request = (FtpWebRequest)WebRequest.Create(uri);
        request.Method = WebRequestMethods.Ftp.DeleteFile;
        request.Credentials = new NetworkCredential(credentials.Username, credentials.Password);

        try
        {
            // Execute the request and get the response
            using (FtpWebResponse response = (FtpWebResponse)await request.GetResponseAsync())
            {
                Debug.WriteLine($"Delete Complete, status {response.StatusDescription}");
                status = $"Successfully deleted {remoteFile}: " + response.StatusDescription;
                return status;  // Successfully deleted the file
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"An error occurred during the FTP delete: {ex.Message}");
            status = "Error: " + ex.Message;
            return status;  // Failed to delete the file
        }
    }

    public static async Task<string> CreateDirectory(FtpCredentials credentials, string remoteDirectory)
    {
        string status = "";

        // Construct the FTP URI for the directory
        string uri = $"{credentials.ServerIp}/{remoteDirectory}";
        Debug.WriteLine("CREATE DIR URI:" + uri);

        // Create an FTPWebRequest object for creating a directory
        FtpWebRequest request = (FtpWebRequest)WebRequest.Create(uri);
        request.Method = WebRequestMethods.Ftp.MakeDirectory;
        request.Credentials = new NetworkCredential(credentials.Username, credentials.Password);

        try
        {
            // Execute the request and get the response
            using (FtpWebResponse response = (FtpWebResponse)await request.GetResponseAsync())
            {
                Debug.WriteLine($"Directory created successfully, status: {response.StatusDescription}");
                status = $"Successfully created {remoteDirectory}: " + response.StatusDescription;
                return status;  // Successfully created the directory
            }
        }
        catch (WebException ex)
        {
            // Tell user if the directory already exists
            if (ex.Response is FtpWebResponse response && response.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable)
            {
                Debug.WriteLine("Directory already exists.");
                status = $"Error: {remoteDirectory} already exists" ;
            }
            else
            {
                Debug.WriteLine($"An error occurred while creating the directory: {ex.Message}");
                status = "Error: " + ex.Message;
            }
            return status;
        }
    }

    public static async Task<string> DeleteDirectory(FtpCredentials credentials, string remoteDirectory)
    {
        string status = "";

        // Construct the FTP URI for the directory
        string uri = $"{credentials.ServerIp}/{remoteDirectory}";

        // Create an FtpWebRequest object for deleting a directory
        FtpWebRequest request = (FtpWebRequest)WebRequest.Create(uri);
        request.Method = WebRequestMethods.Ftp.RemoveDirectory;
        request.Credentials = new NetworkCredential(credentials.Username, credentials.Password);

        try
        {
            // Execute the request and get the response
            using (FtpWebResponse response = (FtpWebResponse)await request.GetResponseAsync())
            {
                Debug.WriteLine($"Directory deleted successfully, status: {response.StatusDescription}");
                status = "Success: " + response.StatusDescription;
                return status; // Successfully deleted the directory
            }
        }
        catch (WebException ex)
        {
            // Handle specific FTP errors, such as directory not empty or directory does not exist
            if (ex.Response is FtpWebResponse response)
            {
                Debug.WriteLine($"Failed to delete directory: {response.StatusDescription}");
                if (response.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable ||
                    response.StatusCode == FtpStatusCode.ActionNotTakenFilenameNotAllowed)
                {
                    Debug.WriteLine("Directory may not exist or is not empty.");
                    status = $"Error: {remoteDirectory} is not empty or does not exist";
                }
            }
            else
            {
                Debug.WriteLine($"An error occurred while deleting the directory: {ex.Message}");
                status = "Error: " + ex.Message;
            }
            return status;  // Failed to delete the directory
        }
    }
}