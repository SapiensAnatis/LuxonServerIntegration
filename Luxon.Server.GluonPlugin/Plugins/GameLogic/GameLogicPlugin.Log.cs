using Microsoft.Extensions.Logging;

namespace Luxon.Server.GluonPlugin.Plugins.GameLogic;

internal sealed partial class GameLogicPlugin
{
    [LoggerMessage(LogLevel.Information, "Room is in solo play mode")]
    partial void LogRoomIsInSoloPlayMode();

    [LoggerMessage(LogLevel.Debug, "Room properties: {@Properties}")]
    partial void LogGameProperties(IDictionary<object, object?> properties);

    [LoggerMessage(
        LogLevel.Information,
        "Viewer ID {ViewerId} created game {GameId} with room ID {RoomId}"
    )]
    partial void LogGameCreated(long viewerId, string gameId, int roomId);
}
