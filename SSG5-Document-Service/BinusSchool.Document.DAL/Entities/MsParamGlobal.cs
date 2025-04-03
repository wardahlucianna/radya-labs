using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.DocumentDb.Abstractions;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.DocumentDb.Entities
{
    public class MsParamGlobal : AuditEntity, IDocumentEntity
    {
        public string Key { get; set; }
        public string Value { get; set; }
        public string Description { get; set; }
    }

    internal class MsParamGlobalConfiguration : AuditEntityConfiguration<MsParamGlobal>
    {
        public override void Configure(EntityTypeBuilder<MsParamGlobal> builder)
        {
            builder.Property(x => x.Key)
                .IsRequired();
            
            builder.Property(x => x.Value)
                .IsRequired();

            builder.Property(x => x.Description)
                .HasMaxLength(128);
            
            base.Configure(builder);
        }
    }
}