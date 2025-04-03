using BinusSchool.Domain.Abstractions;

namespace BinusSchool.Persistence.DocumentDb.Abstractions
{
    public interface IDocumentDbContext : IAppDbContext<IDocumentEntity> { }
}