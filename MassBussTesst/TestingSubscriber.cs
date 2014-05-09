using System;
using System.Collections.Generic;
using System.Threading;

namespace MassBussTesst
{
    class TestingSubscriber : IMessageSubscriber<Message>
    {
        private const int DefaultTimeout = 5000;

        public readonly List<Message> ReceivedMessages = new List<Message>();
        private readonly AutoResetEvent waitHandle = new AutoResetEvent(false);
        private readonly int expectedNoEvents;
        private bool firstTimeException;

        public TestingSubscriber(int expectedNoEvents = 1, bool firstTimeException = false)
        {
            this.expectedNoEvents = expectedNoEvents;
            this.firstTimeException = firstTimeException;
        }

        void IMessageSubscriber<Message>.Handle(Message message)
        {
            System.Diagnostics.Debug.WriteLine("Handle: " + message);

            lock (ReceivedMessages)
            {
                if (firstTimeException)
                {
                    firstTimeException = false;
                    Thread.Sleep(50);
                    throw new Exception();
                }

                ReceivedMessages.Add(message);
                if (ReceivedMessages.Count == expectedNoEvents)
                    waitHandle.Set();
            }
        }

        public void Wait()
        {
            if (!waitHandle.WaitOne(DefaultTimeout))
                throw new Exception("Brak wiadomoœci!");
        }
    }
}