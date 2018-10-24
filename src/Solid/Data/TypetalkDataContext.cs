using Microsoft.EntityFrameworkCore;
using Solid.Data.Entities;

namespace Solid.Data
{
    public class TypetalkDataContext: DbContext
    {
        public const string ConnectionStringName = "TypetalkData";

        public TypetalkDataContext(DbContextOptions<TypetalkDataContext> options)
            : base(options)
        {
        }

        public DbSet<QueryCache> QueryCache { get; set; }
        public DbSet<Post> Post { get; set; }
        public DbSet<Account> Account { get; set; }
        public DbSet<Like> Like { get; set; }
    }
}
