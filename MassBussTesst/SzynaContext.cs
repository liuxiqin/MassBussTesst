using System.Data.Entity;

namespace MassBussTesst
{
    public class SzynaContext : DbContext
    {
        public DbSet<Sekwencja> Sekwencje { get; set; }
    }
}