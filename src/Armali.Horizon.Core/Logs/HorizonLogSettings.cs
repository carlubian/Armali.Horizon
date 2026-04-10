namespace Armali.Horizon.Core.Logs;

public class HorizonLogSettings
{
    public string Endpoint { get; set; } = "http://localhost:5341";
    public string ApiKey { get; set; } = string.Empty;
    public string MinimumLevel { get; set; } = "Information";
    
    /// <summary>
    /// Tamaño máximo de la cola en memoria cuando Seq no está disponible.
    /// Los eventos que superen este límite se descartan silenciosamente.
    /// Por defecto 1000; usar 0 para desactivar el envío a Seq.
    /// </summary>
    public int QueueSizeLimit { get; set; } = 1000;
}