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
            szyna.Publish(new Message());

            // assert
            subsciber.WaitFor(1);
        }

        [Test]
        public void PozwalaWysyłaćSekwencyjnie()
        {
            // arrange
            var szyna = new Szyna();
            var subsciber = new TestingSubscriber();
            szyna.SubscribeOrdered(subsciber);
            szyna.Initialize();

            // act
            szyna.PublishOrdered(new Message { Id = "A" });
            szyna.PublishOrdered(new Message { Id = "B" });
            szyna.PublishOrdered(new Message { Id = "C" });

            // assert
            CollectionAssert.AreEqual(
                new[] { "A", "B", "C" },
                subsciber.WaitFor(3).Select(o => o.Id).ToArray());
        }

        [Test]
        public void WysyłanieJestTransakcyjne_PoRollbackuKomunikatNieDociera()
        {
            // arrange
            var szyna = new Szyna();
            var subsciber = new TestingSubscriber();
            szyna.Subscribe(subsciber);
            szyna.Initialize();

            // act
            using (new TransactionScope())
            {
                szyna.Publish(new Message());
                // no commit!
            }

            // assert
            Assert.Throws<Exception>(() => subsciber.WaitFor(1));
        }

        [Test]
        public void WPrzypadkuBłęduPrzyOdbieraniuKomunikatWracaIJestPonawiany()
        {
            // arrange
            var szyna = new Szyna();
            var subsciber = new TestingSubscriber();
            szyna.Subscribe(subsciber);
            szyna.Initialize();
            subsciber.ThrowExceptionOnce = true;

            // act
            szyna.Publish(new Message());

            // assert
            subsciber.WaitFor(1);
        }

        [Test]
        public void WPrzypadkuPonawianiaKolejnośćKomunikatówNieJestZachowana()
        {
            // arrange
            var szyna = new Szyna();
            var subsciber = new TestingSubscriber();
            szyna.Subscribe(subsciber);
            szyna.Initialize();
            subsciber.ThrowExceptionOnce = true;

            // act
            szyna.Publish(new Message { Id = "A" });
            szyna.Publish(new Message { Id = "B" });

            // assert
            CollectionAssert.AreEqual(
                new[] { "B", "A" },
                subsciber.WaitFor(2).Select(o => o.Id).ToArray());
        }

        [Test]
        public void MożnaWymusićSekwencyjnośćNawetWPrzypadkuPonawiania()
        {
            // arrange
            var szyna = new Szyna();
            var subsciber = new TestingSubscriber();
            szyna.SubscribeOrdered(subsciber);
            szyna.Initialize();
            subsciber.ThrowExceptionOnce = true;

            // act
            szyna.PublishOrdered(new Message { Id = "A" });
            szyna.PublishOrdered(new Message { Id = "B" });

            // assert
            CollectionAssert.AreEqual(
                new[] { "A", "B" },
                subsciber.WaitFor(2).Select(o => o.Id).ToArray());
        }
    }
}