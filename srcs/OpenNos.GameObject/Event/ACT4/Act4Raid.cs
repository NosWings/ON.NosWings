using System;
using System.Linq;
using System.Threading.Tasks;
using CloneExtensions;
using NosSharp.Enums;
using OpenNos.Core;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Map;
using OpenNos.GameObject.Networking;

namespace OpenNos.GameObject.Event.ACT4
{
    public class Act4Raid
    {
        #region Methods

        public async void GenerateRaid(byte type, byte faction)
        {
            ScriptedInstance raid = ServerManager.Instance.Act4Raids.FirstOrDefault(r => r.Id == type);
            MapInstance lobby = ServerManager.Instance.Act4Maps.FirstOrDefault(m => m.Map.MapId == 134);

            if (raid == null || lobby == null)
            {
                Logger.Log.Error(raid == null
                    ? $"Act4 raids is missing - type : {type}"
                    : "There is no map in Act4Maps with MapId == 134");
                return;
            }

            ServerManager.Instance.Act4RaidStart = DateTime.Now;
            lobby.CreatePortal(new Portal
            {
                SourceMapId = 134,
                SourceX = 139,
                SourceY = 100,
                Type = (short)(9 + faction)
            }, 3600, true);

            foreach (MapInstance map in ServerManager.Instance.Act4Maps)
            {
                map.Sessions.Where(s => s?.Character?.Faction == (FactionType)faction).ToList().ForEach(s =>
                    s.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(
                        string.Format(Language.Instance.GetMessageFromKey("ACT4_RAID_OPEN"),
                            ((Act4RaidType)type).ToString()), 0)));
            }

            lock(ServerManager.Instance.FamilyList)
            {
                foreach (Family family in ServerManager.Instance.FamilyList.Where(f => f != null))
                {
                    family.Act4Raid = ServerManager.Instance.Act4Raids.FirstOrDefault(r => r.Id == type)?.GetClone();
                    family.Act4Raid?.LoadScript(MapInstanceType.RaidInstance);
                    if (family.Act4Raid?.FirstMap == null)
                    {
                        continue;
                    }

                    family.Act4Raid.FirstMap.InstanceBag.Lock = true;
                }
            }

            await Task.Delay(60 * 60 * 1000);

            foreach (Family family in ServerManager.Instance.FamilyList.Where(f => f?.Act4Raid != null))
            {
                family.Act4Raid.MapInstanceDictionary?.Values.ToList().ForEach(m => m?.Dispose());
                family.Act4Raid = null;
            }
        }

        #endregion

        #region Singleton

        private static Act4Raid _instance;

        public static Act4Raid Instance => _instance ?? (_instance = new Act4Raid());

        #endregion
    }
}