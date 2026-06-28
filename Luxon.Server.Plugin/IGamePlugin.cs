using Microsoft.Extensions.Configuration;

namespace Luxon.Server.Plugin;

public interface IGamePlugin
{
    Task OnCreateGame(ICreateGameCallInfo info, CancellationToken cancellationToken);

    Task OnJoin(IJoinGameCallInfo info, CancellationToken cancellationToken);

    Task OnRaiseEvent(IRaiseEventCallInfo info, CancellationToken cancellationToken);

    Task BeforeSetProperties(
        IBeforeSetPropertiesCallInfo info,
        CancellationToken cancellationToken
    );
}
