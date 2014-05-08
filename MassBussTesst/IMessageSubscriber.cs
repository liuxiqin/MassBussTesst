namespace MassBussTesst
{
    public interface IMessageSubscriber<in T>
    {
        void Handle(T message);
    }
}