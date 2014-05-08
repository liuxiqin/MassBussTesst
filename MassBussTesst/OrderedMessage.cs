namespace MassBussTesst
{
    public class OrderedMessage<T> where T : class
    {
        public int Number { get; set; }
        public T InnerMessage { get; set; }

        public override string ToString()
        {
            return InnerMessage + " (" + Number + ")";
        }
    }
}