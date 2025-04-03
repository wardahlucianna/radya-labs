using BinusSchool.Domain.Abstractions;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.Entities
{
    /// <summary>
    /// Entity base with Id as Primary Key
    /// </summary>
    public abstract class UniqueEntity : UniquelessEntity
    {

    }

    public class UniqueEntityConfiguration<T> : UniquelessEntityConfiguration<T> where T : UniqueEntity, IEntity
    {
        public override void Configure(EntityTypeBuilder<T> builder)
        {
            // set field Id as Primary Key
            builder.HasKey(p => p.Id);
            
            base.Configure(builder);
        }
    }
}