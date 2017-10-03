

namespace OpenNos.GameObject.BcardsBonus
{
    public class Bonus
    {
        public Bonus()
        {
            // CardType // Additional Type // Data
            Number = new int[200,100,1];
        }

        public int[,,] Number { get; set; }
    }
}
