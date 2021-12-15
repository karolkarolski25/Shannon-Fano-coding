namespace Kodowanie_Shannona_Fano.Models
{
    public class Node
    {
        public Node Left { get; set; }

        public Node Right { get; set; }

        public char Value { get; set; }

        public Node(char Value)
        {
            Left = null;
            Right = null;
            
            this.Value = Value;
        }
    }
}
