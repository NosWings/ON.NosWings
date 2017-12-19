namespace OpenNos.DAL.EF.Entities
{
    public class QuestObjective
    {
        public int QuestObjectiveId { get; set; }

        public int QuestId { get; set; }

        public int? Data { get; set; }

        public int? Objective { get; set; }

        public int? SpecialData { get; set; }

        public int? DropRate { get; set; }
        
        public byte ObjectiveIndex { get; set; }
    }
}
