using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NosSharp.Enums;
using OpenNos.Core;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Map;
using OpenNos.GameObject.Networking;

namespace OpenNos.GameObject.Event.CALIGOR
{
    public static class Caligor
    {
        #region Properties

        public static int AngelDamage { get; set; }

        public static int DemonDamage { get; set; }

        public static MapInstance CaligorMapInstance { get; set; }

        public static MapInstance EntryMap { get; set; }

        public static MapMonster RaidBoss { get; set; }

        #endregion


        #region Methods

        public static void GenerateCaligor()
        {
            ServerManager.Instance.Broadcast(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("CALIGOR_REALM_OPEN"), 0));

            CaligorMapInstance =
                ServerManager.Instance.GenerateMapInstance(154, MapInstanceType.CaligorInstance, new InstanceBag());

            EntryMap = ServerManager.Instance.GetMapInstance(ServerManager.Instance.GetBaseMapInstanceIdByMapId(153));
            EntryMap.CreatePortal(new Portal
            {
                SourceMapId = 153,
                SourceX = 70,
                SourceY = 159,
                DestinationMapId = 0,
                DestinationX = 70,
                DestinationY = 159,
                DestinationMapInstanceId = CaligorMapInstance.MapInstanceId,
                Type = -1
            });
            EntryMap.CreatePortal(new Portal
            {
                SourceMapId = 153,
                SourceX = 110,
                SourceY = 159,
                DestinationMapId = 0,
                DestinationX = 110,
                DestinationY = 159,
                DestinationMapInstanceId = CaligorMapInstance.MapInstanceId,
                Type = -1
            });
            //TODO: Add the top map portal

            RaidBoss = new MapMonster
            {
                MonsterVNum = 2305,
                MapY = 86,
                MapX = 61,
                MapId = CaligorMapInstance.Map.MapId,
                IsMoving = true,
                MapMonsterId = CaligorMapInstance.GetNextId(),
                ShouldRespawn = false
            };
            RaidBoss.Initialize(CaligorMapInstance);
            RaidBoss.BattleEntity.OnDeathEvents.Add(new EventContainer(CaligorMapInstance, EventActionType.SCRIPTEND, 1));
            CaligorMapInstance.AddMonster(RaidBoss);
            CaligorMapInstance.Broadcast(RaidBoss.GenerateIn());

        }

        public static void EndRaid()
        {
            ServerManager.Instance.Broadcast(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("CALIGOR_REALM_CLOSED"), 0));
        }

        public static void LockEntry()
        {
            /*
             * When boss hp = 50%, remove portals
             */
        }

        #endregion
    }
}