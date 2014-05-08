using System;
using System.Messaging;
using MassTransit;
using MassTransit.Advanced;
using MassTransit.SubscriptionConfigurators;

namespace MassBussTesst
{
    class Szyna
    {
        private Action<SubscriptionBusServiceConfigurator> subscribeAction;

        public void Publish<T>(T message) where T : class
        {
            Bus.Instance.Publish(message);
        }

        public void Subscribe<T>(IMessageSubscriber<T> subscriber) where T : class
        {
            subscribeAction = subs => subs.Handler<T>(subscriber.Handle).Permanent();
        }

        public void Initialize()
        {
            var address = new Uri("msmq://localhost/created_transactional");
            const string localName = @".\private$\created_transactional";

            if (MessageQueue.Exists(localName))
                MessageQueue.Delete(localName);

            Bus.Initialize(
                sbc =>
                {
                    sbc.UseMsmq(o => o.UseMulticastSubscriptionClient());
                    sbc.SetConcurrentReceiverLimit(1);
                    sbc.ReceiveFrom(address);
                    sbc.SetCreateMissingQueues(true);
                    sbc.SetCreateTransactionalQueues(true);
                    sbc.Subscribe(subs => subscribeAction(subs));
                });
        }

        public static void Shutdown()
        {
            Bus.Shutdown();
        }
    }
}