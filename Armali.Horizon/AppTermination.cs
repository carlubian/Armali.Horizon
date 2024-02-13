namespace Armali.Horizon;

/// <summary>
/// Class used by the Horizon host to terminate the app from within a hosted service.
/// </summary>
/// <param name="token">A Cancellation Token source used to run the host</param>
public class AppTermination(CancellationTokenSource token)
{
    private readonly CancellationTokenSource _token = token;

    /// <summary>
    /// Requests a termination of the app. This will stop all services and exit the app.
    /// </summary>
    public void Terminate()
    {
        _token.Cancel();
    }
}
