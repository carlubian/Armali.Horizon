using Azure.Storage;
using Azure.Storage.Files.DataLake;

namespace Armali.Horizon.Autoconfig.Services;

/// <summary>
/// Servicio de almacenamiento de ficheros de configuración en Azure Data Lake Gen2.
/// Estructura de directorios: {nodeName}/{appName}/{versionName}/{fileName}
/// </summary>
public class AutoconfigDatalakeService
{
    private static DataLakeServiceClient? ServiceClient;
    private static DataLakeFileSystemClient? FileSystemClient;
    
    // Credentials — la clave se lee de la variable de entorno DATALAKE_ACCOUNT_KEY
    private static string AccountName = "armali";
    private static string ContainerName = "autoconfig";

    /// <summary>
    /// Indica si el servicio está disponible (la clave de Data Lake estaba configurada
    /// al momento de la primera inicialización).
    /// </summary>
    public bool IsAvailable { get; private set; }

    public AutoconfigDatalakeService()
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
        ServiceClient = new DataLakeServiceClient(new Uri($"https://{AccountName}.blob.core.windows.net/{ContainerName}"), sharedKeyCredential);
        FileSystemClient = ServiceClient.GetFileSystemClient(ContainerName);
        IsAvailable = true;
    }

    /// <summary>Lanza si el servicio no tiene credenciales configuradas.</summary>
    private void EnsureAvailable()
    {
        if (!IsAvailable)
            throw new InvalidOperationException(
                "La variable de entorno 'DATALAKE_ACCOUNT_KEY' no está configurada o está vacía.");
    }

    /// <summary>
    /// Sube un fichero al Datalake, sobreescribiéndolo si ya existe.
    /// </summary>
    public async Task UploadFileAsync(Stream content, string nodeName, string appName, string versionName, string fileName)
    {
        EnsureAvailable();
        var DirectoryPath = $"{nodeName}/{appName}/{versionName}";
        var DirClient = FileSystemClient!.GetDirectoryClient(DirectoryPath);
        await DirClient.CreateIfNotExistsAsync();
        var FileClient = DirClient.GetFileClient(fileName);
        await FileClient.UploadAsync(content, overwrite: true);
    }

    /// <summary>
    /// Descarga un fichero del Datalake como Stream.
    /// </summary>
    public async Task<Stream> GetFileAsync(string nodeName, string appName, string versionName, string fileName)
    {
        EnsureAvailable();
        var DirectoryPath = $"{nodeName}/{appName}/{versionName}";
        var DirClient = FileSystemClient!.GetDirectoryClient(DirectoryPath);
        var FileClient = DirClient.GetFileClient(fileName);
        var DownloadResponse = await FileClient.ReadAsync();
        return DownloadResponse.Value.Content;
    }

    /// <summary>
    /// Elimina un fichero del Datalake.
    /// </summary>
    public async Task DeleteFileAsync(string nodeName, string appName, string versionName, string fileName)
    {
        EnsureAvailable();
        var DirectoryPath = $"{nodeName}/{appName}/{versionName}";
        var DirClient = FileSystemClient!.GetDirectoryClient(DirectoryPath);
        var FileClient = DirClient.GetFileClient(fileName);
        await FileClient.DeleteIfExistsAsync();
    }
}

