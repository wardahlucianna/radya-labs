using BinusSchool.Domain.Abstractions;

namespace BinusSchool.Domain.NoEntities
{
    public abstract class UniqueNoEntity : AuditableNoEntity, INoEntity
    {
        public string Id { get; set; }
    }
}