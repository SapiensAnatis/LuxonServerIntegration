using Microsoft.Extensions.Configuration;

namespace Luxon.Server.Plugin;

public interface IGamePlugin
{
    public Task OnCreateGame(ICreateGameCallInfo info, CancellationToken cancellationToken);

    public void BeforeJoinGame();

    public void OnJoinGame();
}
