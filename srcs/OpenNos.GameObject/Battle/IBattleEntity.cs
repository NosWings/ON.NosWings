using OpenNos.GameObject.Map;
using NosSharp.Enums;

namespace OpenNos.GameObject.Battle
{
    public interface IBattleEntity
    {
        MapCell GetPos();

        BattleEntity GetInformations();

        AttackType GetAttackType(Skill skill = null);

        object GetSession();
    }
}
