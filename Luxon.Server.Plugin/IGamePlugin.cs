namespace Luxon.Server.Plugin;

public interface IGamePlugin
{
    public Task OnCreateGame(ICreateGameCallInfo info);

    public void BeforeJoinGame();

    public void OnJoinGame();
}
