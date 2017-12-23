using OpenNos.GameObject.Map;
using NosSharp.Enums;
using OpenNos.PathFinder.PathFinder;
using OpenNos.GameObject.Packets.ServerPackets;

namespace OpenNos.GameObject.Battle
{
    public interface IBattleEntity
    {
        #region Properties

        FactionType Faction { get; }

        int CurrentHp { get; set; }

        int MaxHp { get; }

        MapInstance MapInstance { get; }

        BattleEntity BattleEntity { get; set; }

        #endregion

        #region Methods

        Node[,] GetBrushFire();

        MapCell GetPos();

        EffectPacket GenerateEff(int effectid);

        AttackType GetAttackType(Skill skill = null);

        SessionType SessionType();

        void GetDamage(int damage, bool canKill = true);

        void GenerateDeath(IBattleEntity killer = null);

        void GenerateRewards(IBattleEntity target);

        bool IsTargetable(SessionType type, bool isPvP = false);

        long GetId();

        object GetSession();

        #endregion
    }
}
