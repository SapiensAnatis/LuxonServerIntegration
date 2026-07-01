using Luxon.Server.GluonPlugin.Shared;
using Luxon.Server.Plugin;

namespace Luxon.Server.GluonPlugin.Plugins.Gluon;

internal sealed class GluonPlugin(
    IPluginHost host,
    PluginStateService pluginStateService,
    GameLogic.GameLogicPlugin gameLogicPlugin
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

    public override async Task OnJoin(IJoinGameCallInfo info, CancellationToken cancellationToken)
    {
        await gameLogicPlugin.OnJoin(info, cancellationToken);
        // this.stateManagerPlugin.OnJoin(info);

        if (!info.IsProcessed)
        {
            info.Continue();
        }
    }

    public override async Task OnLeave(ILeaveGameCallInfo info, CancellationToken cancellationToken)
    {
        await gameLogicPlugin.OnLeave(info, cancellationToken);
        // this.stateManagerPlugin.OnLeave(info);

        if (!info.IsProcessed)
        {
            // Normally you should call base.OnLeave() here to allow master client handovers, but Dragalia doesn't
            // implement this and instead nukes the room when the master client leaves.
            info.Continue();
        }
    }

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

    public override async Task OnRaiseEvent(
        IRaiseEventCallInfo info,
        CancellationToken cancellationToken
    )
    {
        await gameLogicPlugin.OnRaiseEvent(info, cancellationToken);
        // this.stateManagerPlugin.OnRaiseEvent(info);

        if (!info.IsProcessed)
        {
            info.Continue();
        }
    }

    public override async Task BeforeSetProperties(
        IBeforeSetPropertiesCallInfo info,
        CancellationToken cancellationToken
    )
    {
        await gameLogicPlugin.BeforeSetProperties(info, cancellationToken);

        if (!info.IsProcessed)
        {
            info.Continue();
        }
    }

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
