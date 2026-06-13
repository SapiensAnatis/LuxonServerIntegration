using Luxon.Server.Plugin;

namespace Luxon.Server.GluonPlugin;

internal sealed class GluonPlugin : GamePlugin
{
    public GluonPlugin()
    {
        Console.WriteLine("GluonPlugin: Constructor");
    }

    public override PluginResult OnAttach()
    {
        Console.WriteLine("GluonPlugin: OnAttach");
        return PluginResult.Continue;
    }

    public override void OnCreateGame(ICreateGameCallInfo info)
    {
        Console.WriteLine("GluonPlugin: OnCreateGame");

        // object? playerId = info.Request.ActorProperties["PlayerId"];
        //
        // Console.WriteLine($"PlayerId: {playerId}");
        //
        // info.Request.ActorProperties.Add("ViewerId", playerId);

        // info.Continue();
    }

    public override PluginResult BeforeJoinGame()
    {
        Console.WriteLine("GluonPlugin: BeforeJoinGame");
        return PluginResult.Continue;
    }

    public override PluginResult OnJoinGame()
    {
        Console.WriteLine("GluonPlugin: OnJoinGame");
        return PluginResult.Continue;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        Console.WriteLine("Disposing GluonPlugin");
    }
}
