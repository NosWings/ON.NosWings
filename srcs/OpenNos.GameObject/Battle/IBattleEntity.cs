using OpenNos.GameObject.Map;
using NosSharp.Enums;
using OpenNos.PathFinder.PathFinder;
using OpenNos.GameObject.Packets.ServerPackets;

namespace OpenNos.GameObject.Battle
{
    public interface IBattleEntity
    {
        Node[,] GetBrushFire();

        MapCell GetPos();

        MapInstance GetMapInstance();

        BattleEntity GetInformations();

        EffectPacket GenerateEff(int effectid);

        AttackType GetAttackType(Skill skill = null);

        SessionType GetSessionType();

        void GetDamage(int damage, bool canKill = true);

        void GenerateDeath(IBattleEntity killer = null);

        void GenerateRewards(IBattleEntity target);

        bool isTargetable(SessionType type, bool isPvP = false);

        int GetCurrentHp();

        int GetMaxHp();

        long GetId();

        object GetSession();
    }
}
