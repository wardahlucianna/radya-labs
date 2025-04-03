using BinusSchool.Domain.Abstractions;

namespace BinusSchool.Persistence.SchoolDb.Abstractions
{
    public interface ISchoolDbContext : IAppDbContext<ISchoolEntity> { }
}