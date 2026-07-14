using System.Collections;
using DragaliaAPI.Photon.Shared.Enums;
using DragaliaAPI.Photon.Shared.Models;
using Luxon.Server.GluonPlugin.Shared.Constants;
using Luxon.Server.GluonPlugin.Shared.Models;
using MessagePack;

namespace Luxon.Server.GluonPlugin.Shared.Helpers;

internal static class DtoHelpers
{
    public static Player CreatePlayer(int actorNr, IDictionary<object, object?> actorProperties)
    {
        return new Player()
        {
            ActorNr = actorNr,
            ViewerId = actorProperties.GetInt(ActorPropertyKeys.PlayerId),
            PartyNoList = (int[])actorProperties[ActorPropertyKeys.UsePartySlot],
        };
    }

    public static GameBase CreateGame(string name, IDictionary<object, object?> gameProperties)
    {
        GameBase result = new GameBase
        {
            Name = name,
            MatchingCompatibleId = gameProperties.GetInt(GamePropertyKeys.MatchingCompatibleId),
            RoomId = gameProperties.GetInt(GamePropertyKeys.RoomId),
            QuestId = gameProperties.GetInt(GamePropertyKeys.QuestId),
            MatchingType = (MatchingTypes)gameProperties.GetInt(GamePropertyKeys.MatchingType),
        };

        EntryConditions? conditions = CreateEntryConditions(gameProperties);

        if (conditions != null)
        {
            result.EntryConditions = conditions;
        }

        return result;
    }

    public static EntryConditions? CreateEntryConditions(
        IDictionary<object, object?> gameProperties
    )
    {
        if (
            !gameProperties.TryGetValue(
                GamePropertyKeys.EntryConditions,
                out object? entryConditionObj
            )
        )
        {
            return null;
        }

        if (entryConditionObj is not byte[] entryConditionBlob)
        {
            return null;
        }

        RoomEntryCondition deserialized = MessagePackSerializer.Deserialize<RoomEntryCondition>(
            entryConditionBlob,
            MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4Block)
        );

        return new EntryConditions()
        {
            UnacceptedElementTypeList = deserialized.UnacceptedElementals,
            UnacceptedWeaponTypeList = deserialized.UnacceptedWeapons,
            ObjectiveTextId = deserialized.Objective.TextId,
            RequiredPartyPower = deserialized.RequiredPower,
        };
    }
}
