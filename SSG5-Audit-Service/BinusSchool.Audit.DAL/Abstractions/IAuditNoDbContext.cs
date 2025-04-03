using BinusSchool.Domain.Abstractions;

namespace BinusSchool.Persistence.AuditDb.Abstractions
{
    public interface IAuditNoDbContext : IAppNoDbContext<IAuditNoEntity> { }
}