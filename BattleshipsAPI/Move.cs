namespace BattleshipsAPI
{
    public class Move
    {        
        public string PlayerName { get; set; } 
        public int X { get; set; }
        public int Y { get; set; }

        public override string ToString()
        {
            return string.Format("{0} {1} {2}", PlayerName, X, Y);
        }
    }
}
