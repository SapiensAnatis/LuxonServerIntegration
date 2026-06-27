using System.Diagnostics;
using Luxon.Server.Plugin;
using Microsoft.Extensions.Configuration;

namespace Luxon.Server.GluonPlugin;

internal sealed class GluonPlugin(
    IPluginHost host,
    PluginStateService pluginStateService,
    GameLogicPlugin gameLogicPlugin
) : PluginBase(host)
{
    public override async Task OnCreateGame(
        ICreateGameCallInfo info,
        CancellationToken cancellationToken
    )
    {
        await gameLogicPlugin.OnCreateGame(info, cancellationToken);
        // this.stateManagerPlugin.OnCreateGame(info);
        //
        // if (pluginStateService.IsPubliclyVisible)
        // {
        //     this.discordPlugin?.OnCreateGame(info);
        // }
        //
        if (!info.IsProcessed)
        {
            info.Continue();
        }
    }

    // public override void OnJoin(IJoinGameCallInfo info)
    // {
    //     gameLogicPlugin.OnJoin(info);
    //     // this.stateManagerPlugin.OnJoin(info);
    //
    //     if (!info.IsProcessed)
    //     {
    //         info.Continue();
    //     }
    // }

    // public override void OnLeave(ILeaveGameCallInfo info)
    // {
    //     gameLogicPlugin.OnLeave(info);
    //     // this.stateManagerPlugin.OnLeave(info);
    //
    //     if (!info.IsProcessed)
    //     {
    //         base.OnLeave(info);
    //     }
    // }

    // public override void BeforeCloseGame(IBeforeCloseGameCallInfo info)
    // {
    //     // This can't use OnCloseGame with StateManagerPlugin, as there can only be one synchronous outbound HTTP request
    //     // per event handler. (Unless we chain them together using callbacks...)
    //
    //     if (pluginStateService.IsPubliclyVisible)
    //     {
    //         // this.discordPlugin?.BeforeCloseGame(info);
    //     }
    //
    //     if (!info.IsProcessed)
    //     {
    //         info.Continue();
    //     }
    // }

    // public override void OnCloseGame(ICloseGameCallInfo info)
    // {
    //     // GameLogicPlugin has no override for closing a game
    //     // this.stateManagerPlugin.OnCloseGame(info);
    //
    //     if (!info.IsProcessed)
    //     {
    //         info.Continue();
    //     }
    // }

    // public override void OnRaiseEvent(IRaiseEventCallInfo info)
    // {
    //     gameLogicPlugin.OnRaiseEvent(info);
    //     // this.stateManagerPlugin.OnRaiseEvent(info);
    //
    //     if (!info.IsProcessed)
    //     {
    //         info.Continue();
    //     }
    // }

    // public override void BeforeSetProperties(IBeforeSetPropertiesCallInfo info)
    // {
    //     gameLogicPlugin.BeforeSetProperties(info);
    //
    //     if (!info.IsProcessed)
    //     {
    //         info.Continue();
    //     }
    // }

    // public override void OnSetProperties(ISetPropertiesCallInfo info)
    // {
    //     gameLogicPlugin.OnSetProperties(info);
    //     // this.stateManagerPlugin.OnSetProperties(info);
    //
    //     if (pluginStateService.IsPubliclyVisible)
    //     {
    //         // this.discordPlugin?.OnSetProperties(info);
    //     }
    //
    //     if (!info.IsProcessed)
    //     {
    //         info.Continue();
    //     }
    // }
}
