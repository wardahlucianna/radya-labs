using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Domain.Abstractions
{
    public interface IAppNoDbContext<TNoEntity> : IAppDbContext
        where TNoEntity : class, INoEntity
    {
        DbSet<UEntity> NoEntity<UEntity>() where UEntity : class, TNoEntity;
    }
}