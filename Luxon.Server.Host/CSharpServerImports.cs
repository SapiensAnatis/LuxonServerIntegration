using System.Diagnostics.CodeAnalysis;
using Luxon.Ser;
using Luxon.Server.Host.Collections;
using Luxon.Server.Host.Enums.DictKeyCodes;
using Luxon.Server.Host.Plugin;
using Luxon.Server.Plugin;

namespace Luxon.Server.Host;

internal sealed class CSharpServerImports(PluginManager pluginManager, GameManager gameManager)
{
    public void GamePluginOnAttach(uint pluginId, Game game)
    {
        if (!pluginManager.TryCreatePlugin(pluginId, out IGamePlugin? plugin))
        {
            throw new InvalidOperationException($"Failed to create plugin with ID {pluginId}");
        }

        gameManager.AddPluginToGame(game.DangerousHandle, pluginId, plugin);
    }

    public LuxonGamePluginResult GamePluginOnCreateGame(
        uint pluginId,
        Game game,
        SerMessageRef request,
        Peer creator,
        bool isJoin,
        bool createIfNotExist
    )
    {
        if (!gameManager.TryGetGamePlugin(game.DangerousHandle, pluginId, out IGamePlugin? plugin))
        {
            throw new InvalidOperationException($"Failed to retrieve plugin with ID {pluginId}");
        }

        Message managedRequest = request.ToManagedMessage();

        if (
            managedRequest.Payload is not OperationRequestMessage opr
            || !opr.Parameters.TryGetValue(
                (byte)Properties.ActorProperties,
                out Value actorProperties
            )
        )
        {
            throw new InvalidOperationException("Unexpected message type");
        }

        if (
            actorProperties
            is not { TypeTag: 21, Content: IDictionary<Value, Value> actorPropertiesDictionary }
        )
        {
            throw new InvalidOperationException("Unexpected actor properties type");
        }

        var proxy = new TypelessLuxonDictionaryProxy(actorPropertiesDictionary);

        JoinGameRequest pluginRequest = new() { ActorProperties = proxy };

        CreateGameCallInfo info = new()
        {
            CreateIfNotExists = createIfNotExist,
            Request = pluginRequest,
        };

        plugin.OnCreateGame(info);

        ThrowIfNoResultSet(info.Result);

        request.Replace(managedRequest);

        return info.Result.Value;
    }

    public LuxonGamePluginResult GamePluginOnCloseGame(
        uint pluginId,
        Game game,
        bool failedOnCreate
    )
    {
        // TODO: this doesn't fire for disconnects due to errors

        gameManager.RemovePlugin(game.DangerousHandle, pluginId);

        return LuxonGamePluginResult.Continue;
    }

    public static void ThrowIfNoResultSet([NotNull] LuxonGamePluginResult? result)
    {
        if (result == null)
        {
            throw new InvalidOperationException(
                "Plugin callback did not call any processing result method on ICallInfo"
            );
        }
    }
}
