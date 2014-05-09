using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace MassBussTesst
{
    class TestingSubscriber : IMessageSubscriber<Message>
    {
        private const int DefaultTimeout = 5000;

        private readonly BlockingCollection<Message> receivedMessages
            = new BlockingCollection<Message>(new ConcurrentQueue<Message>());

        public bool ThrowExceptionOnce { private get; set; }

        void IMessageSubscriber<Message>.Handle(Message message)
        {
            System.Diagnostics.Debug.WriteLine("Handle: " + message);

            lock(receivedMessages)
            {
                if (ThrowExceptionOnce)
                {
                    ThrowExceptionOnce = false;
                    Thread.Sleep(50);
                    throw new Exception();
                }

                receivedMessages.Add(message);
            }
        }

        public List<Message> WaitFor(int numberOfMessages)
        {
            var messages = new List<Message>();

            while (numberOfMessages-- > 0)
            {
                Message msg;
                if (!receivedMessages.TryTake(out msg, DefaultTimeout))
                    throw new Exception("Brak wiadomo�ci!");

                messages.Add(msg);
            }

            return messages;
        }
    }
}