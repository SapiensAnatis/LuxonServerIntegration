namespace Luxon.Server.Plugin;

public abstract class GamePlugin : IDisposable
{
    public abstract PluginResult OnAttach();
    public abstract void OnCreateGame(ICreateGameCallInfo info);
    public abstract PluginResult BeforeJoinGame();
    public abstract PluginResult OnJoinGame();

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing) { }
}
