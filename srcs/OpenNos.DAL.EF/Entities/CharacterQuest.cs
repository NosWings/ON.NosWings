using OpenNos.DAL.EF.Entities.Base;

namespace OpenNos.DAL.EF.Entities
{
    public class CharacterQuest : SynchronizableBaseEntity
    {
        #region Properties

        public virtual Character Character { get; set; }

        public long CharacterId { get; set; }

        public virtual Quest Quest { get; set; }

        public long QuestId { get; set; }

        public int FirstObjective { get; set; }

        public int SecondObjective { get; set; }

        public int ThirdObjective { get; set; }

        public int FourthObjective { get; set; }

        public int FifthObjective { get; set; }

        public bool IsMainQuest { get; set; }

        #endregion
    }
}