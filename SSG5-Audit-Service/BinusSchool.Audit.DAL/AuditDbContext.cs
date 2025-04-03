using BinusSchool.Domain.Abstractions;
using BinusSchool.Persistence.AuditDb.Abstractions;
using BinusSchool.Persistence.Extensions;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Persistence.AuditDb
{
    public class AuditDbContext : AppDbContext<AuditDbContext>, IAuditNoDbContext
    {
        public AuditDbContext(DbContextOptions<AuditDbContext> options)
            : base(options) { }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyEntityRegistration2();
        }

        DbSet<UEntity> IAppNoDbContext<IAuditNoEntity>.NoEntity<UEntity>()
        {
            return Set<UEntity>();
        }
    }
}