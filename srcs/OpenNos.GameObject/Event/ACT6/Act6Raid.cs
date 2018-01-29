using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NosSharp.Enums;
using OpenNos.Core;
using OpenNos.GameObject.Map;
using OpenNos.GameObject.Networking;

namespace OpenNos.GameObject.Event.ACT6
{
    public static class Act6Raid
    {
        #region Properties
        public static ScriptedInstance RaidInstance;

        public static MapInstance EntryMap;
        #endregion

        #region Methods
        public static void GenerateRaid(FactionType raidType)
        {
            RaidInstance =
                ServerManager.Instance.Act6Raids.FirstOrDefault(s => s.Id == (raidType == FactionType.Angel ? 23 : 24));

            if (RaidInstance == null)
            {
                Logger.Log.Info(Language.Instance.GetMessageFromKey("CANT_CREATE_RAIDS"));
                return;
            }

            EntryMap = ServerManager.Instance.GetMapInstance(
                ServerManager.Instance.GetBaseMapInstanceIdByMapId(RaidInstance.MapId));

            if (EntryMap == null)
            {
                Logger.Log.Info(Language.Instance.GetMessageFromKey("MAP_MISSING"));
                return;
            }

           EntryMap.CreatePortal(new Portal
           {
               Type = (byte)PortalType.Raid,
               SourceMapId = RaidInstance.MapId,
               SourceX = RaidInstance.PositionX,
               SourceY = RaidInstance.PositionY
           }, 3600, true);
        }
        #endregion
    }
}