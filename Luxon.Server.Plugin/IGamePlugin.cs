namespace Luxon.Server.Plugin;

public interface IGamePlugin
{
    public void OnCreateGame(ICreateGameCallInfo info);

    public void BeforeJoinGame();

    public void OnJoinGame();
}
