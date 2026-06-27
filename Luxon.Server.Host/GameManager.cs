using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Luxon.Server.Plugin;

namespace Luxon.Server.Host;

internal sealed class GameManager
{
    private readonly Dictionary<nint, GameAdditionalFields> additionalFields = [];

    public ConcurrentQueue<TaskContext> CompletedTaskQueue { get; } = new();

    public ConcurrentQueue<InfoCallbackContext> CommandRestartQueue { get; } = new();

    public ThreadLocal<InfoCallbackContext?> CurrentCallbackContext { get; } = new();

    public void AddPluginToGame(nint gameHandle, uint pluginId, IGamePlugin pluginInstance)
    {
        ref var gameAdditionalFields = ref CollectionsMarshal.GetValueRefOrAddDefault(
            this.additionalFields,
            gameHandle,
            out bool exists
        );

        if (exists && gameAdditionalFields != null)
        {
            gameAdditionalFields.Plugins.Add(pluginId, pluginInstance);
        }
        {
            gameAdditionalFields = new GameAdditionalFields()
            {
                Plugins = { [pluginId] = pluginInstance },
            };
        }
    }

    public bool TryGetGamePlugin(
        nint gameHandle,
        uint pluginId,
        [NotNullWhen(true)] out IGamePlugin? pluginInstance
    )
    {
        pluginInstance = null;

        return this.additionalFields.TryGetValue(gameHandle, out GameAdditionalFields? fields)
            && fields.Plugins.TryGetValue(pluginId, out pluginInstance);
    }

    public void RemovePlugin(nint gameHandle, uint pluginId)
    {
        if (!this.additionalFields.TryGetValue(gameHandle, out GameAdditionalFields? fields))
        {
            return;
        }

        bool removed = fields.Plugins.Remove(pluginId);
        Debug.Assert(removed);

        if (fields.Plugins.Count == 0)
        {
            this.additionalFields.Remove(gameHandle);
        }
    }
}
