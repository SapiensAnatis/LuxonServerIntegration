using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Luxon.Server.Plugin;

namespace Luxon.Server.Host;

internal class GameManager
{
    private readonly Dictionary<nint, GameAdditionalFields> _additionalFields = [];

    public void AddPluginToGame(nint gameHandle, uint pluginId, GamePlugin pluginInstance)
    {
        ref var gameAdditionalFields = ref CollectionsMarshal.GetValueRefOrAddDefault(
            _additionalFields,
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
        [NotNullWhen(true)] out GamePlugin? pluginInstance
    )
    {
        pluginInstance = null;

        return _additionalFields.TryGetValue(gameHandle, out GameAdditionalFields? fields)
            && fields.Plugins.TryGetValue(pluginId, out pluginInstance);
    }

    public void RemovePlugin(nint gameHandle, uint pluginId)
    {
        if (_additionalFields.TryGetValue(gameHandle, out GameAdditionalFields? fields))
        {
            bool removed = fields.Plugins.Remove(pluginId);
            Debug.Assert(removed);

            if (fields.Plugins.Count == 0)
            {
                _additionalFields.Remove(gameHandle);
            }
        }
    }
}
