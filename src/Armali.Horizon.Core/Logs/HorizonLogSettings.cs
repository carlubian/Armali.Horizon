namespace Armali.Horizon.Core.Logs;

public class HorizonLogSettings
{
    public string Endpoint { get; set; } = "http://localhost:5341";
    public string ApiKey { get; set; } = string.Empty;
    public string MinimumLevel { get; set; } = "Information";
}