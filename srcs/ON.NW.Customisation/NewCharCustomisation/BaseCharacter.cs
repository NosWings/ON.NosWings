using NosSharp.Enums;
using OpenNos.Data;

namespace ON.NW.Customisation.NewCharCustomisation
{
    public class BaseCharacter
    {
        public BaseCharacter() => Character = new CharacterDTO
        {
            Class = ClassType.Adventurer,
            Gender = GenderType.Male,
            HairColor = HairColorType.Black,
            HairStyle = HairStyleType.HairStyleA,
            Hp = 221,
            JobLevel = 20,
            Level = 15,
            MapId = 1,
            MapX = 78,
            MapY = 109,
            Mp = 221,
            MaxMateCount = 10,
            Gold = 15000,
            SpPoint = 10000,
            SpAdditionPoint = 0,
            Name = "template",
            Slot = 0,
            AccountId = 0,
            MinilandMessage = "Welcome",
            State = CharacterState.Active
        };

        public CharacterDTO Character { get; set; }
    }
}