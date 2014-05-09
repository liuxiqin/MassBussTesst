using System;
using System.Linq;
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
            var subsciber = new TestingSubscriber();
            szyna.Subscribe(subsciber);
            szyna.Initialize();

            // act
            szyna.Publish(Message.Create());

            // assert
            subsciber.WaitFor(1);
        }

        [Test]
        public void WysłaneKomunikatyDocierająDoCeluWKolejnościWysyłaniaGdyUżytyJest1Wątek()
        {
            // arrange
            var szyna = new Szyna();
            var subsciber = new TestingSubscriber();
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
            CollectionAssert.AreEqual(
                new[] { message1.Id, message2.Id, message3.Id },
                subsciber.WaitFor(3).Select(o => o.Id));
        }

        [Test]
        public void WysyłanieJestTransakcyjne_PoRollbackuKomunikatNieDociera()
        {
            // arrange
            var szyna = new Szyna();
            var subsciber = new TestingSubscriber();
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
            Assert.Throws<Exception>(() => subsciber.WaitFor(1));
        }

        [Test]
        public void OdbieranieJestTransakcyjne()
        {
            // arrange
            var szyna = new Szyna();
            var subsciber = new TestingSubscriber();
            szyna.Subscribe(subsciber);
            szyna.Initialize();
            subsciber.ThrowExceptionOnce = true;
            var message = Message.Create();

            // act
            using (var ts = new TransactionScope())
            {
                szyna.Publish(message);
                ts.Complete();
            }

            // assert
            // może niezbyt jasno widać, ale komunikat doszedł za drugim razem...
            subsciber.WaitFor(1);
        }

        [Test]
        public void WPrzypadkuPonawianiaKolejnośćKomunikatówNieJestZachowana()
        {
            // arrange
            var szyna = new Szyna();
            var subsciber = new TestingSubscriber();
            szyna.Subscribe(subsciber);
            szyna.Initialize(concurrent: true);
            subsciber.ThrowExceptionOnce = true;
            var message1 = Message.Create();
            var message2 = Message.Create();

            // act
            szyna.Publish(message1);
            szyna.Publish(message2);

            // assert
            CollectionAssert.AreEqual(
                new[] { message2.Id, message1.Id },
                subsciber.WaitFor(2).Select(o => o.Id).ToArray());
        }

        [Test]
        public void Ordered_WPrzypadkuPonawianiaKolejnośćKomunikatówJestZachowana()
        {
            // arrange
            var szyna = new Szyna();
            var subsciber = new TestingSubscriber();
            szyna.SubscribeOrdered(subsciber);
            szyna.Initialize(concurrent: true);
            subsciber.ThrowExceptionOnce = true;
            var message1 = Message.Create();
            var message2 = Message.Create();

            // act
            szyna.PublishOrdered(message1);
            szyna.PublishOrdered(message2);

            // assert
            CollectionAssert.AreEqual(
                new[] { message1.Id, message2.Id },
                subsciber.WaitFor(2).Select(o => o.Id).ToArray());
        }
    }
}