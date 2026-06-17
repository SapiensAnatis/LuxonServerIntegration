using System.Diagnostics;
using Luxon.Server.Plugin;

namespace Luxon.Server.GluonPlugin;

internal sealed class GluonPlugin : IGamePlugin
{
    public GluonPlugin()
    {
        Console.WriteLine("GluonPlugin: Constructor");
    }

    public async Task OnCreateGame(ICreateGameCallInfo info, CancellationToken token)
    {
        Console.WriteLine("GluonPlugin: OnCreateGame");

        object? playerId = info.Request.ActorProperties["PlayerId"];

        Console.WriteLine($"PlayerId: {playerId}");

        info.Request.ActorProperties.Add("ViewerId", playerId);

        await Task.Delay(1000, token);

        info.Continue();

        await Task.Delay(1000, token);

        Console.WriteLine("GluonPlugin: Done waiting");
    }

    public void BeforeJoinGame()
    {
        Console.WriteLine("GluonPlugin: BeforeJoinGame");
    }

    public void OnJoinGame()
    {
        Console.WriteLine("GluonPlugin: OnJoinGame");
    }
}
