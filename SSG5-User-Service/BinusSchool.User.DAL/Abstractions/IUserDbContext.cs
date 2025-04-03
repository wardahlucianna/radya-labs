using BinusSchool.Domain.Abstractions;

namespace BinusSchool.Persistence.UserDb.Abstractions
{
    public interface IUserDbContext : IAppDbContext<IUserEntity> { }
}