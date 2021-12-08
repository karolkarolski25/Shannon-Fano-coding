namespace Kodowanie_Shannona_Fano
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

    public class CharStatistics
    {
        public char Char { get; set; }

        public int Count { get; set; }
    }

    public class CodeWord
    {
        public char Char { get; set; }

        public string Code { get; set; }
    }

    public class Summary : CodeWord
    {
        public int Count { get; set; }
    }
}
