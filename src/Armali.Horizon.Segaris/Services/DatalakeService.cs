using Azure.Storage;
using Azure.Storage.Files.DataLake;

namespace Armali.Horizon.Segaris.Services;

public class DatalakeService
{
    private static DataLakeServiceClient? ServiceClient;
    private static DataLakeFileSystemClient? FileSystemClient;
    private static DataLakeDirectoryClient? DirectoryClient;
    
    // Credentials — la clave se lee de la variable de entorno DATALAKE_ACCOUNT_KEY
    private static string AccountName = "armali";
    private static string ContainerName = "segaris";
    private static string ArchiveDirectoryName = "ArchiveStorage";
    
    /// <summary>
    /// Indica si el servicio está disponible (la clave de Data Lake estaba configurada
    /// al momento de la primera inicialización).
    /// </summary>
    public bool IsAvailable { get; private set; }

    public DatalakeService()
    {
        var accountKey = Environment.GetEnvironmentVariable("DATALAKE_ACCOUNT_KEY");
        if (string.IsNullOrEmpty(accountKey))
        {
            // Sin clave no se puede operar, pero no impedimos que el resto del sistema
            // funcione. Las operaciones de Data Lake lanzarán cuando se invoquen.
            IsAvailable = false;
            return;
        }

        var sharedKeyCredential = new StorageSharedKeyCredential(AccountName, accountKey);

        // Create DataLakeServiceClient using StorageSharedKeyCredentials
        ServiceClient = new DataLakeServiceClient(new Uri($"https://{AccountName}.blob.core.windows.net/{ContainerName}"), sharedKeyCredential);
        FileSystemClient = ServiceClient.GetFileSystemClient(ContainerName);
        DirectoryClient = FileSystemClient.GetDirectoryClient(ArchiveDirectoryName);
        DirectoryClient.CreateIfNotExists();
        IsAvailable = true;
    }

    /// <summary>Lanza si el servicio no tiene credenciales configuradas.</summary>
    private void EnsureAvailable()
    {
        if (!IsAvailable)
            throw new InvalidOperationException(
                "La variable de entorno 'DATALAKE_ACCOUNT_KEY' no está configurada o está vacía.");
    }

    // Archive Methods
    public async Task UploadFileAsync(Stream content, string fileName)
    {
        EnsureAvailable();
        var FileClient = DirectoryClient!.GetFileClient(fileName);
        await FileClient.UploadAsync(content, overwrite: true);
    }

    public async Task DeleteFileAsync(string fileName)
    {
        EnsureAvailable();
        var FileClient = DirectoryClient!.GetFileClient(fileName);
        await FileClient.DeleteIfExistsAsync();
    }

    public async Task<Stream> GetFileAsync(string fileName)
    {
        EnsureAvailable();
        var FileClient = DirectoryClient!.GetFileClient(fileName);
        var DownloadResponse = await FileClient.ReadAsync();
        return DownloadResponse.Value.Content;
    }

    // Project Methods
    public async Task UploadProjectFileAsync(Stream content, string fileName, string programName, string axisName, string projectCode)
    {
        EnsureAvailable();
        var DirectoryPath = $"{programName}/{axisName}/{projectCode}";
        var DirClient = FileSystemClient!.GetDirectoryClient(DirectoryPath);
        await DirClient.CreateIfNotExistsAsync();
        var FileClient = DirClient.GetFileClient(fileName);
        await FileClient.UploadAsync(content, overwrite: true);
    }

    public async Task DeleteProjectFileAsync(string fileName, string programName, string axisName, string projectCode)
    {
        EnsureAvailable();
        var DirectoryPath = $"{programName}/{axisName}/{projectCode}";
        var DirClient = FileSystemClient!.GetDirectoryClient(DirectoryPath);
        var FileClient = DirClient.GetFileClient(fileName);
        await FileClient.DeleteIfExistsAsync();
    }

    public async Task<Stream> GetProjectFileAsync(string fileName, string programName, string axisName, string projectCode)
    {
        EnsureAvailable();
        var DirectoryPath = $"{programName}/{axisName}/{projectCode}";
        var DirClient = FileSystemClient!.GetDirectoryClient(DirectoryPath);
        var FileClient = DirClient.GetFileClient(fileName);
        var DownloadResponse = await FileClient.ReadAsync();
        return DownloadResponse.Value.Content;
    }
}