using System;
using System.Linq;
using System.Transactions;

namespace MassBussTesst
{
    class SekwencjeService : ISekwencjeService
    {
        private static readonly object SyncObject = new Object();

        public int NastepnaWartosc(string nazwaSekwencji)
        {
            using (var db = new SzynaContext())
            using (var ts = new TransactionScope())
            {
                lock(SyncObject)
                {
                    var s = db.Sekwencje.FirstOrDefault(o => o.Nazwa == nazwaSekwencji);
                    if (s == null)
                    {
                        s = new Sekwencja { Nazwa = nazwaSekwencji, Wartosc = 0 };
                        db.Sekwencje.Add(s);
                    }
                    s.Wartosc++;
                    db.SaveChanges();
                    ts.Complete();
                    return s.Wartosc;
                }
            }
        }
    }
}