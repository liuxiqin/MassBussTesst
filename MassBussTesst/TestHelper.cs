using System.Data.Entity;
using System.Messaging;

namespace MassBussTesst
{
    public static class TestHelper
    {
        public static void InitializeDatabase()
        {
            using (var db = new SzynaContext())
            {
                new DropCreateDatabaseIfModelChanges<SzynaContext>().InitializeDatabase(db);
                db.Sekwencje.RemoveRange(db.Sekwencje);
                db.SaveChanges();
            }
        }

        public static void DeleteQueues()
        {
            if (MessageQueue.Exists(@".\private$\" + Szyna.QueueName))
                MessageQueue.Delete(@".\private$\" + Szyna.QueueName);
        }
    }
}