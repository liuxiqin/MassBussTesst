using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Transactions;
using NUnit.Framework;

namespace MassBussTesst
{
    [TestFixture]
    public class SzynaTests
    {
        [TearDown]
        public void TearDown()
        {
            Szyna.Shutdown();
        }

        [Test]
        public void WysłanyKomunikatDocieraDoCelu()
        {
            // arrange
            var szyna = new Szyna();
            var subsciber = new Subscriber();
            szyna.Subscribe(subsciber);
            szyna.Initialize();
            var message = Message.Create();

            // act
            szyna.Publish(message);

            // assert
            subsciber.Wait();
        }

        [Test]
        public void WysłaneKomunikatyDocierająDoCeluWKolejnościWysyłaniaGdyUżytyJest1Wątek()
        {
            // arrange
            var szyna = new Szyna();
            var subsciber = new Subscriber(expectedNoEvents: 3);
            szyna.Subscribe(subsciber);
            szyna.Initialize(concurrent: false);
            var message1 = Message.Create();
            var message2 = Message.Create();
            var message3 = Message.Create();

            // act
            szyna.Publish(message1);
            szyna.Publish(message2);
            szyna.Publish(message3);

            // assert
            subsciber.Wait();
            CollectionAssert.AreEqual(
                new[] { message1.Id, message2.Id, message3.Id },
                subsciber.ReceivedMessages.Select(o => o.Id));
        }

        [Test]
        public void WysyłanieJestTransakcyjne_PoRollbackuKomunikatNieDociera()
        {
            // arrange
            var szyna = new Szyna();
            var subsciber = new Subscriber();
            szyna.Subscribe(subsciber);
            szyna.Initialize();
            var message = Message.Create();

            // act
            using (new TransactionScope())
            {
                szyna.Publish(message);
                // no commit!
            }

            // assert
            Assert.Throws<Exception>(subsciber.Wait);
        }

        [Test]
        public void OdbieranieJestTransakcyjne()
        {
            // arrange
            var szyna = new Szyna();
            var subsciber = new Subscriber(expectedNoEvents: 1, firstTimeException: true);
            szyna.Subscribe(subsciber);
            szyna.Initialize();
            var message = Message.Create();

            // act
            using (var ts = new TransactionScope())
            {
                szyna.Publish(message);
                ts.Complete();
            }

            // assert
            // może niezbyt jasno widać, ale komunikat doszedł za drugim razem...
            subsciber.Wait();
        }

        [Test]
        public void WPrzypadkuPonawianiaKolejnośćKomunikatówNieJestZachowana()
        {
            // arrange
            var szyna = new Szyna();
            var subsciber = new Subscriber(expectedNoEvents: 2, firstTimeException: true);
            szyna.Subscribe(subsciber);
            szyna.Initialize(concurrent: true);
            var message1 = Message.Create();
            var message2 = Message.Create();

            // act
            szyna.Publish(message1);
            szyna.Publish(message2);

            // assert
            subsciber.Wait();
            CollectionAssert.AreEqual(
                new[] { message2.Id, message1.Id },
                subsciber.ReceivedMessages.Select(o => o.Id).ToArray());
        }

        [Test]
        public void Ordered_WPrzypadkuPonawianiaKolejnośćKomunikatówJestZachowana()
        {
            // arrange
            var szyna = new Szyna();
            var subsciber = new Subscriber(expectedNoEvents: 2, firstTimeException: true);
            szyna.SubscribeOrdered(subsciber);
            szyna.Initialize(concurrent: true);
            var message1 = Message.Create();
            var message2 = Message.Create();

            // act
            szyna.PublishOrdered(message1);
            szyna.PublishOrdered(message2);

            // assert
            subsciber.Wait();
            CollectionAssert.AreEqual(
                new[] { message1.Id, message2.Id },
                subsciber.ReceivedMessages.Select(o => o.Id).ToArray());
        }

        private class Subscriber : IMessageSubscriber<Message>
        {
            private const int DefaultTimeout = 5000;

            public readonly List<Message> ReceivedMessages = new List<Message>();
            private readonly AutoResetEvent waitHandle = new AutoResetEvent(false);
            private readonly int expectedNoEvents;
            private bool firstTimeException;

            public Subscriber(int expectedNoEvents = 1, bool firstTimeException = false)
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
                        throw new Exception();
                    }

                    Thread.Sleep(50);

                    ReceivedMessages.Add(message);
                    if (ReceivedMessages.Count == expectedNoEvents)
                        waitHandle.Set();
                }
            }

            public void Wait()
            {
                if (!waitHandle.WaitOne(DefaultTimeout))
                    throw new Exception("Brak wiadomości!");
            }
        }

        private class Message
        {
            public string Id { get; set; }

            public static Message Create()
            {
                return new Message { Id = Guid.NewGuid().ToString() };
            }

            public override string ToString()
            {
                return Id;
            }
        }
    }
}