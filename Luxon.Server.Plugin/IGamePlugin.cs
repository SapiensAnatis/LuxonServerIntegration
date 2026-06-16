namespace Luxon.Server.Plugin;

public interface IGamePlugin
{
    public Task OnCreateGame(
        ICreateGameCallInfo info,
        CancellationToken cancellationToken = default
    );

    public void BeforeJoinGame();

    public void OnJoinGame();
}
