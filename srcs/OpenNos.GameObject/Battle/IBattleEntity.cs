using OpenNos.GameObject.Map;
using NosSharp.Enums;
using OpenNos.PathFinder.PathFinder;
using OpenNos.GameObject.Packets.ServerPackets;

namespace OpenNos.GameObject.Battle
{
    public interface IBattleEntity
    {
        MapCell GetPos();

        MapInstance GetMapInstance();

        BattleEntity GetInformations();

        AttackType GetAttackType(Skill skill = null);

        object GetSession();

        bool isTargetable(SessionType type, bool isPvP = false);

        Node[,] GetBrushFire();

        SessionType GetSessionType();

        long GetId();

        void GetDamage(int damage, bool canKill = true);

        EffectPacket GenerateEff(int effectid);

        void GenerateDeath(IBattleEntity killer = null);

        void GenerateRewards(IBattleEntity target);

        int[] GetHp();
    }
}
