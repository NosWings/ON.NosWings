﻿using OpenNos.Core;
using OpenNos.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OpenNos.GameObject.Event
{
    public class EventHelper
    {
        public static MapInstance GenerateLod()
        {
            int lodtime = 120;
            int HornTime = 30;
            int HornRepawn = 4;
            int HornStay = 1;
            MapInstance LandOfDeath = ServerManager.GenerateMapInstance(150, MapInstanceType.LodInstance);
            LandOfDeath.StartClock((int)(TimeSpan.FromMinutes(lodtime).TotalSeconds * 10));
            Observable.Timer(TimeSpan.FromMinutes(lodtime- HornTime)).Subscribe(x => {  LandOfDeath.XpRate = 3; LandOfDeath.DropRate = 3; });
            Observable.Timer(TimeSpan.FromMinutes(lodtime - HornTime), TimeSpan.FromMinutes(HornRepawn)).Subscribe(
                x =>
                {
                    Character lastincharacter = LandOfDeath.GetLastInCharacter();
                    List<Tuple<short, short, short, long>> SummonParameters = new List<Tuple<short, short, short, long>>();
                    SummonParameters.Add(new Tuple<short, short, short, long>(443,( lastincharacter != null ? lastincharacter.PositionX : (short)154),( lastincharacter != null ? lastincharacter.PositionY: (short)140), lastincharacter !=null?lastincharacter.CharacterId: -1));
                    LandOfDeath.Sessions.ToList().ForEach(s => s.SendPacket("df 2"));
                    LandOfDeath.Sessions.ToList().ForEach(s => s.SendPacket(s.Character.GenerateMsg(Language.Instance.GetMessageFromKey("HORN_APPEAR"), 0)));
                    List<int> monsterIds = LandOfDeath.SummonMonster(SummonParameters);
                    Observable.Timer(TimeSpan.FromMinutes(HornStay)).Subscribe(c =>
                    {
                        LandOfDeath.Lock = true;
                        LandOfDeath.Sessions.ToList().ForEach(s => s.SendPacket(s.Character.GenerateMsg(Language.Instance.GetMessageFromKey("HORN_DISAPEAR"), 0)));
                        LandOfDeath.UnspawnMonsters(monsterIds);
                    });
                });
            Observable.Timer(TimeSpan.FromMinutes(lodtime)).Subscribe(x => { LandOfDeath.Dispose(); ServerManager.Instance.EnableMapEffect(98, false); });
            return LandOfDeath;
        }

        public static TimeSpan GetMilisecondsBeforeTime(TimeSpan time)
        {
            TimeSpan day = time;    // 24 hours in a day.
            TimeSpan now = TimeSpan.Parse(DateTime.Now.ToString("HH:mm"));     // The current time in 24 hour format
            TimeSpan timeLeftUntilFirstRun = ((day - now));
            if (timeLeftUntilFirstRun.TotalHours > 24)
                timeLeftUntilFirstRun -= new TimeSpan(24, 0, 0);
            return timeLeftUntilFirstRun;
        }
    }
}
