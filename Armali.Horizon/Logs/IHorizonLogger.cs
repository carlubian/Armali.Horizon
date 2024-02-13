namespace Armali.Horizon.Logs;

public interface IHorizonLogger
{
    public void Trace(string template, params object[] segments);
    public void Info(string template, params object[] segments);
    public void Warning(string template, params object[] segments);
    public void Error(string template, params object[] segments);
}
