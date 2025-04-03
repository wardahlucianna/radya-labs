using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.DocumentDb.Abstractions;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Persistence.DocumentDb.Entities
{
    public class MsSourceData : CodeEntity, IDocumentEntity
    {
        public string IdSchool { get; set; }
        public string Endpoint { get; set; }
        public virtual MsSchool School { get; set; }
    }

    internal class MsSourceDataConfiguration : CodeEntityConfiguration<MsSourceData>
    {
        public override void Configure(EntityTypeBuilder<MsSourceData> builder)
        {
            builder.Property(x => x.IdSchool)
                .HasMaxLength(36)
                .IsRequired();

            builder.HasOne(x => x.School)
              .WithMany(x => x.SourceDatas)
              .HasForeignKey(fk => fk.IdSchool)
              .HasConstraintName("FK_MsSourceData_MsSchool")
              .OnDelete(DeleteBehavior.Cascade)
              .IsRequired();

            builder.Property(x => x.Endpoint)
                .IsRequired();

            base.Configure(builder);

            builder.Property(x => x.Description)
                .IsRequired(false);
        }
    }
}
