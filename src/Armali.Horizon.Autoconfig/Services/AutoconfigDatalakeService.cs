using Azure.Storage;
using Azure.Storage.Files.DataLake;

namespace Armali.Horizon.Autoconfig.Services;

/// <summary>
/// Servicio de almacenamiento de ficheros de configuración en Azure Data Lake Gen2.
/// Estructura de directorios: {nodeName}/{appName}/{versionName}/{fileName}
/// </summary>
public class AutoconfigDatalakeService
{
#pragma warning disable CS8618
    // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
    private static DataLakeServiceClient ServiceClient;
    private static DataLakeFileSystemClient FileSystemClient;
#pragma warning restore CS8618
    
    // Credentials — la clave se lee de la variable de entorno DATALAKE_ACCOUNT_KEY
    private static string AccountName = "armali";
    private static string ContainerName = "autoconfig";

    public AutoconfigDatalakeService()
    {
        var AccountKey = Environment.GetEnvironmentVariable("DATALAKE_ACCOUNT_KEY")
            ?? throw new InvalidOperationException("La variable de entorno 'DATALAKE_ACCOUNT_KEY' no está configurada.");

        var SharedKeyCredential = new StorageSharedKeyCredential(AccountName, AccountKey);

        ServiceClient = new DataLakeServiceClient(new Uri($"https://{AccountName}.blob.core.windows.net/{ContainerName}"), SharedKeyCredential);
        FileSystemClient = ServiceClient.GetFileSystemClient(ContainerName);
    }

    /// <summary>
    /// Sube un fichero al Datalake, sobreescribiéndolo si ya existe.
    /// </summary>
    public async Task UploadFileAsync(Stream content, string nodeName, string appName, string versionName, string fileName)
    {
        var DirectoryPath = $"{nodeName}/{appName}/{versionName}";
        var DirClient = FileSystemClient.GetDirectoryClient(DirectoryPath);
        await DirClient.CreateIfNotExistsAsync();
        var FileClient = DirClient.GetFileClient(fileName);
        await FileClient.UploadAsync(content, overwrite: true);
    }

    /// <summary>
    /// Descarga un fichero del Datalake como Stream.
    /// </summary>
    public async Task<Stream> GetFileAsync(string nodeName, string appName, string versionName, string fileName)
    {
        var DirectoryPath = $"{nodeName}/{appName}/{versionName}";
        var DirClient = FileSystemClient.GetDirectoryClient(DirectoryPath);
        var FileClient = DirClient.GetFileClient(fileName);
        var DownloadResponse = await FileClient.ReadAsync();
        return DownloadResponse.Value.Content;
    }

    /// <summary>
    /// Elimina un fichero del Datalake.
    /// </summary>
    public async Task DeleteFileAsync(string nodeName, string appName, string versionName, string fileName)
    {
        var DirectoryPath = $"{nodeName}/{appName}/{versionName}";
        var DirClient = FileSystemClient.GetDirectoryClient(DirectoryPath);
        var FileClient = DirClient.GetFileClient(fileName);
        await FileClient.DeleteIfExistsAsync();
    }
}

