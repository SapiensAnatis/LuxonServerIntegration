using System.Net;
using Luxon.Ser;
using Luxon.Server.GluonPlugin.Plugins.GameLogic.Events;
using Microsoft.Extensions.Logging;

namespace Luxon.Server.GluonPlugin.Plugins.GameLogic;

internal partial class GameLogicPlugin
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

    [LoggerMessage(
        LogLevel.Warning,
        "Rejecting join request -- game already has {CurrentActorCount} actors"
    )]
    partial void LogRejectingJoinRequestGameFull(int currentActorCount);

    [LoggerMessage(LogLevel.Information, "Rejecting join request -- room is already in progress")]
    partial void LogRejectingJoinGameAlreadyInProgress();

    [LoggerMessage(LogLevel.Information, "Rejecting join request -- room has no host")]
    partial void LogRejectingJoinGameHasNoHost();

    [LoggerMessage(LogLevel.Information, "Viewer ID {ViewerId} joined game {GameId}")]
    partial void LogPlayerJoined(long viewerId, string gameId);

    [LoggerMessage(
        LogLevel.Information,
        "Commencing random matching start timer for game {GameId}"
    )]
    partial void LogCommencingRandomMatchingTimer(string gameId);

    [LoggerMessage(LogLevel.Information, "Executing random matching start for game {GameId}")]
    partial void LogExecutingRandomMatchingStart(string gameId);

    [LoggerMessage(LogLevel.Trace, "Event properties: {@Parameters}")]
    partial void LogEventProperties(Dictionary<byte, Value> parameters);

    [LoggerMessage(LogLevel.Trace, "Actor {ActorNr} raised event: {EventCode} ({EventCodeHex:X})")]
    partial void LogEventRaised(int actorNr, Event eventCode, byte eventCodeHex);

    [LoggerMessage(LogLevel.Information, "Received Ready event from actor {ActorNr}")]
    partial void LogReceivedReadyEventFromActor(int actorNr);

    [LoggerMessage(
        LogLevel.Information,
        "Not registering TA clear -- quest {QuestId} is not ranked"
    )]
    partial void LogNotRegisteringTaClear(int questId);

    [LoggerMessage(
        LogLevel.Information,
        "Not registering TA clear -- game actor count {GameActorCount} < 4"
    )]
    partial void LogNotRegisteringNonFullTaClear(int gameActorCount);

    [LoggerMessage(LogLevel.Information, "Registering time attack clear")]
    partial void LogRegisteringTimeAttackClear();

    [LoggerMessage(LogLevel.Information, "Received GameSucceed from actor {ActorNr}")]
    partial void LogReceivedGameSucceed(int actorNr);

    [LoggerMessage(LogLevel.Debug, "Received FailQuestRequest with FailType {FailType}")]
    partial void LogReceivedFailQuestRequest(FailQuestRequest.FailTypes failType);

    [LoggerMessage(LogLevel.Error, "Time attack record request failed with status {Status}")]
    partial void LogTimeAttackRecordRequestFailed(HttpStatusCode status);

    [LoggerMessage(LogLevel.Debug, "FailQuestRequest: returning to lobby")]
    partial void LogFailQuestReturningToLobby();
}
