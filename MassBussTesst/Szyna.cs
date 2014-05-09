using System;
using System.Collections.Generic;
using System.Messaging;
using MassTransit;
using MassTransit.SubscriptionConfigurators;

namespace MassBussTesst
{
    class Szyna
    {
        private readonly List<Action<SubscriptionBusServiceConfigurator>> subscribtions
            = new List<Action<SubscriptionBusServiceConfigurator>>();

        private int sequencer;

        public void PublishOrdered<T>(T message) where T : class
        {
            // TODO: locking?
            Publish(new OrderedMessage<T> { Number = ++sequencer, InnerMessage = message });
        }

        public void Publish<T>(T message) where T : class
        {
            System.Diagnostics.Debug.WriteLine("Publish: " + message);
            Bus.Instance.Publish(message);
        }

        public void SubscribeOrdered<T>(IMessageSubscriber<T> subscriber) where T : class
        {
            Subscribe(new OrderedSubsciber<T>(subscriber));
        }

        public void Subscribe<T>(IMessageSubscriber<T> subscriber) where T : class
        {
            subscribtions.Add(subs => subs.Handler<T>(subscriber.Handle).Permanent());
        }

        private class OrderedSubsciber<T> : IMessageSubscriber<OrderedMessage<T>>
            where T : class
        {
            private static readonly object SyncObject = new Object();
            private int lastMessageNumber;
            private readonly IMessageSubscriber<T> subscriber;

            public OrderedSubsciber(IMessageSubscriber<T> subscriber)
            {
                this.subscriber = subscriber;
            }

            public void Handle(OrderedMessage<T> message)
            {
                lock(SyncObject)
                {
                    if (message.Number == lastMessageNumber + 1)
                    {
                        subscriber.Handle(message.InnerMessage);
                        lastMessageNumber++;
                    }
                    else
                    {
                        // TODO: odpowiednik HandleCurrentMessageLater z NSeviceBus?
                        throw new Exception("Out of order!");
                    }
                }
            }
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
                    sbc.ReceiveFrom(address);
                    sbc.SetDefaultRetryLimit(10);
                    sbc.SetCreateMissingQueues(true);
                    sbc.SetCreateTransactionalQueues(true);
                    sbc.Subscribe(subs => subscribtions.ForEach(action => action(subs)));
                });
        }

        public static void Shutdown()
        {
            Bus.Shutdown();
        }
    }
}