using System;
using System.Linq;
using System.Transactions;
using NUnit.Framework;

namespace MassBussTesst
{
    [TestFixture]
    public class SzynaTests
    {
        private Szyna szyna;
        private TestingSubscriber subscriber;

        [SetUp]
        public void SetUpEachTest()
        {
            TestHelper.InitializeDatabase();
            szyna = new Szyna(new SekwencjeService());
            subscriber = new TestingSubscriber();
        }

        [TearDown]
        public void TearDownEachTest()
        {
            szyna.Dispose();
            TestHelper.DeleteQueues();
        }

        [Test]
        public void WysłanyKomunikatDocieraDoCelu()
        {
            // arrange
            szyna.Subscribe(subscriber);
            szyna.Initialize();

            // act
            szyna.Publish(new Message());

            // assert
            subscriber.WaitFor(1);
        }

        [Test]
        public void PozwalaWysyłaćSekwencyjnie()
        {
            // arrange
            szyna.SubscribeOrdered(subscriber);
            szyna.Initialize();

            // act
            using (var ts = new TransactionScope())
            {
                szyna.PublishOrdered(new Message { Id = "A" });
                szyna.PublishOrdered(new Message { Id = "B" });
                szyna.PublishOrdered(new Message { Id = "C" });
                ts.Complete();
            }

            // assert
            CollectionAssert.AreEqual(
                new[] { "A", "B", "C" },
                subscriber.WaitFor(3).Select(o => o.Id).ToArray());
        }

        [Test]
        public void WysyłanieJestTransakcyjne_PoRollbackuKomunikatNieDociera()
        {
            // arrange
            szyna.Subscribe(subscriber);
            szyna.Initialize();

            // act
            using (new TransactionScope())
            {
                szyna.Publish(new Message());
                // no commit!
            }

            // assert
            Assert.Throws<Exception>(() => subscriber.WaitFor(1));
        }

        [Test]
        public void WPrzypadkuBłęduPrzyOdbieraniuKomunikatWracaIJestPonawiany()
        {
            // arrange
            szyna.Subscribe(subscriber);
            szyna.Initialize();
            subscriber.ThrowExceptionOnce = true;

            // act
            using (var ts = new TransactionScope())
            {
                szyna.Publish(new Message());
                ts.Complete();
            }

            // assert
            subscriber.WaitFor(1);
        }

        [Test, Ignore]
        public void WPrzypadkuPonawianiaKolejnośćKomunikatówNieJestZachowana()
        {
            // arrange
            szyna.Subscribe(subscriber);
            szyna.Initialize();
            subscriber.ThrowExceptionOnce = true;

            // act
            szyna.Publish(new Message { Id = "A" });
            szyna.Publish(new Message { Id = "B" });

            // assert
            CollectionAssert.AreEqual(
                new[] { "B", "A" },
                subscriber.WaitFor(2).Select(o => o.Id).ToArray());
        }

        [Test]
        public void MożnaWymusićSekwencyjnośćNawetWPrzypadkuPonawiania()
        {
            // arrange
            szyna.SubscribeOrdered(subscriber);
            szyna.Initialize();
            subscriber.ThrowExceptionOnce = true;

            // act
            using (var ts = new TransactionScope())
            {
                szyna.PublishOrdered(new Message { Id = "A" });
                szyna.PublishOrdered(new Message { Id = "B" });
                ts.Complete();
            }

            // assert
            CollectionAssert.AreEqual(
                new[] { "A", "B" },
                subscriber.WaitFor(2).Select(o => o.Id).ToArray());
        }
    }
}