using Luxon.Server.Plugin;

namespace Luxon.Server.GluonPlugin;

internal sealed class GluonPlugin : IGamePlugin
{
    public GluonPlugin()
    {
        Console.WriteLine("GluonPlugin: Constructor");
    }

    public void OnCreateGame(ICreateGameCallInfo info)
    {
        Console.WriteLine("GluonPlugin: OnCreateGame");

        object? playerId = info.Request.ActorProperties["PlayerId"];

        Console.WriteLine($"PlayerId: {playerId}");

        info.Request.ActorProperties.Add("ViewerId", playerId);

        info.Continue();
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
