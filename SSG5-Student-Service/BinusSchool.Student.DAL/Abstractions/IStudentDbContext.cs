using BinusSchool.Domain.Abstractions;

namespace BinusSchool.Persistence.StudentDb.Abstractions
{
    public interface IStudentDbContext : IAppDbContext<IStudentEntity> { }
}