using System.Data.Entity;
using System.Transactions;
using NUnit.Framework;

namespace MassBussTesst
{
    [TestFixture]
    public class SekwencjeServiceTests
    {
        [SetUp]
        public void SetUpEachTest()
        {
            using (var db = new SzynaContext())
            {
                new DropCreateDatabaseIfModelChanges<SzynaContext>().InitializeDatabase(db);
                db.Sekwencje.RemoveRange(db.Sekwencje);
                db.SaveChanges();
            }
        }

        [Test]
        public void DajKolejneNumeryPoczynającOd1()
        {
            // arrange
            var service = new SekwencjeService();

            // act & assert
            CollectionAssert.AreEqual(
                new[] { 1, 2, 3 },
                new[]
                {
                    service.NastepnaWartosc("A"),
                    service.NastepnaWartosc("A"),
                    service.NastepnaWartosc("A")
                });
        }

        [Test]
        public void JestTransakcyjny()
        {
            // arrange
            var service = new SekwencjeService();

            // act & assert
            Assert.AreEqual(1, service.NastepnaWartosc("A"));

            using (new TransactionScope())
            {
                Assert.AreEqual(2, service.NastepnaWartosc("A"));
                Assert.AreEqual(3, service.NastepnaWartosc("A"));
            }

            Assert.AreEqual(2, service.NastepnaWartosc("A"));
        }
    }
}