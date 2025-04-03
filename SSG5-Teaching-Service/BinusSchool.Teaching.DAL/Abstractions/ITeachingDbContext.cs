using BinusSchool.Domain.Abstractions;

namespace BinusSchool.Persistence.TeachingDb.Abstractions
{
    public interface ITeachingDbContext : IAppDbContext<ITeachingEntity> { }
}