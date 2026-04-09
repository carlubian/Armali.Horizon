using Azure.Storage;
using Azure.Storage.Files.DataLake;

namespace Armali.Horizon.Segaris.Services;

public class DatalakeService
{
#pragma warning disable CS8618
    // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
    private static DataLakeServiceClient ServiceClient;
    private static DataLakeFileSystemClient FileSystemClient;
    private static DataLakeDirectoryClient DirectoryClient;
#pragma warning restore CS8618
    
    // Credentials — la clave se lee de la variable de entorno DATALAKE_ACCOUNT_KEY
    private static string AccountName = "armali";
    private static string ContainerName = "segaris";
    private static string ArchiveDirectoryName = "ArchiveStorage";

    public DatalakeService()
    {
        var AccountKey = Environment.GetEnvironmentVariable("DATALAKE_ACCOUNT_KEY")
            ?? throw new InvalidOperationException("La variable de entorno 'DATALAKE_ACCOUNT_KEY' no está configurada.");

        var SharedKeyCredential = new StorageSharedKeyCredential(AccountName, AccountKey);

        // Create DataLakeServiceClient using StorageSharedKeyCredentials
        ServiceClient = new DataLakeServiceClient(new Uri($"https://{AccountName}.blob.core.windows.net/{ContainerName}"), SharedKeyCredential);
        FileSystemClient = ServiceClient.GetFileSystemClient(ContainerName);
        DirectoryClient = FileSystemClient.GetDirectoryClient(ArchiveDirectoryName);
        DirectoryClient.CreateIfNotExists();
    }

    // Archive Methods
    public async Task UploadFileAsync(Stream content, string fileName)
    {
        var FileClient = DirectoryClient.GetFileClient(fileName);
        await FileClient.UploadAsync(content, overwrite: true);
    }

    public async Task DeleteFileAsync(string fileName)
    {
        var FileClient = DirectoryClient.GetFileClient(fileName);
        await FileClient.DeleteIfExistsAsync();
    }

    public async Task<Stream> GetFileAsync(string fileName)
    {
        var FileClient = DirectoryClient.GetFileClient(fileName);
        var DownloadResponse = await FileClient.ReadAsync();
        return DownloadResponse.Value.Content;
    }

    // Project Methods
    public async Task UploadProjectFileAsync(Stream content, string fileName, string programName, string axisName, string projectCode)
    {
        var DirectoryPath = $"{programName}/{axisName}/{projectCode}";
        var DirClient = FileSystemClient.GetDirectoryClient(DirectoryPath);
        await DirClient.CreateIfNotExistsAsync();
        var FileClient = DirClient.GetFileClient(fileName);
        await FileClient.UploadAsync(content, overwrite: true);
    }

    public async Task DeleteProjectFileAsync(string fileName, string programName, string axisName, string projectCode)
    {
        var DirectoryPath = $"{programName}/{axisName}/{projectCode}";
        var DirClient = FileSystemClient.GetDirectoryClient(DirectoryPath);
        var FileClient = DirClient.GetFileClient(fileName);
        await FileClient.DeleteIfExistsAsync();
    }

    public async Task<Stream> GetProjectFileAsync(string fileName, string programName, string axisName, string projectCode)
    {
        var DirectoryPath = $"{programName}/{axisName}/{projectCode}";
        var DirClient = FileSystemClient.GetDirectoryClient(DirectoryPath);
        var FileClient = DirClient.GetFileClient(fileName);
        var DownloadResponse = await FileClient.ReadAsync();
        return DownloadResponse.Value.Content;
    }
}