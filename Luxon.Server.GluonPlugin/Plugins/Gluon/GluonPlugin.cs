using Luxon.Server.GluonPlugin.Plugins.GameLogic;
using Luxon.Server.GluonPlugin.Plugins.StateManager;
using Luxon.Server.GluonPlugin.Shared;
using Luxon.Server.Plugin;

namespace Luxon.Server.GluonPlugin.Plugins.Gluon;

internal sealed class GluonPlugin : PluginBase
{
    private readonly GameLogicPlugin gameLogicPlugin;
    private readonly StateManagerPlugin stateManagerPlugin;

    public GluonPlugin(
        IPluginHost host,
        PluginStateService pluginStateService,
        GameLogicPlugin gameLogicPlugin,
        StateManagerPlugin stateManagerPlugin
    )
        : base(host)
    {
        this.gameLogicPlugin = gameLogicPlugin;
        this.stateManagerPlugin = stateManagerPlugin;
    }

    public override async Task OnCreateGame(
        ICreateGameCallInfo info,
        CancellationToken cancellationToken
    )
    {
        await this.gameLogicPlugin.OnCreateGame(info, cancellationToken);
        await this.stateManagerPlugin.OnCreateGame(info, cancellationToken);

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
        await this.gameLogicPlugin.OnJoin(info, cancellationToken);
        await this.stateManagerPlugin.OnJoin(info, cancellationToken);

        if (!info.IsProcessed)
        {
            info.Continue();
        }
    }

    public override async Task OnLeave(ILeaveGameCallInfo info, CancellationToken cancellationToken)
    {
        await this.gameLogicPlugin.OnLeave(info, cancellationToken);
        await this.stateManagerPlugin.OnLeave(info, cancellationToken);

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

    public override async Task OnCloseGame(
        ICloseGameCallInfo info,
        CancellationToken cancellationToken
    )
    {
        // GameLogicPlugin has no override for closing a game
        await this.stateManagerPlugin.OnCloseGame(info, cancellationToken);

        if (!info.IsProcessed)
        {
            info.Continue();
        }
    }

    public override async Task OnRaiseEvent(
        IRaiseEventCallInfo info,
        CancellationToken cancellationToken
    )
    {
        await this.gameLogicPlugin.OnRaiseEvent(info, cancellationToken);
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
        await this.gameLogicPlugin.BeforeSetProperties(info, cancellationToken);

        if (!info.IsProcessed)
        {
            info.Continue();
        }
    }

    public override async Task OnSetProperties(
        ISetPropertiesCallInfo info,
        CancellationToken cancellationToken
    )
    {
        // GameLogicPlugin has no OnSetProperties logic
        await this.stateManagerPlugin.OnSetProperties(info, cancellationToken);

        // if (pluginStateService.IsPubliclyVisible)
        // {
        //     // this.discordPlugin?.OnSetProperties(info);
        // }
        //
        if (!info.IsProcessed)
        {
            info.Continue();
        }
    }
}
