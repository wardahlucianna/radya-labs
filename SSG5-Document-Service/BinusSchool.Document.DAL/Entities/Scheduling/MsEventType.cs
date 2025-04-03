using System.Collections.Generic;
using BinusSchool.Persistence.DocumentDb.Abstractions;
using BinusSchool.Persistence.DocumentDb.Entities.School;
using BinusSchool.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.DocumentDb.Entities
{
    public class MsEventType : CodeEntity, IDocumentEntity
    {
        public string IdAcademicYear { get; set; }

        public virtual ICollection<TrEvent> Events { get; set; }
        public virtual MsAcademicYear AcademicYear { get; set; }
    }

    internal class MsEventTypeConfiguration : CodeEntityConfiguration<MsEventType>
    {
        public override void Configure(EntityTypeBuilder<MsEventType> builder)
        {
            builder.HasOne(x => x.AcademicYear)
               .WithMany(x => x.EventTypes)
               .HasForeignKey(fk => fk.IdAcademicYear)
               .HasConstraintName("FK_MsEventType_MsAcademicYear")
               .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
