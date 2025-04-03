using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.Persistence.SchoolDb.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchoolDb.Entities
{
    public class MsAnswerSet : AuditEntity, ISchoolEntity
    {
        public string IdAcademicYear { get; set; }
        public string AnswerSetName { get; set; }
        public virtual MsAcademicYear AcademicYear { get; set; }
        public virtual ICollection<MsAnswerSetOption> AnswerSetOptions { get; set; }
    }

    internal class MsAnswerSetConfiguration : AuditEntityConfiguration<MsAnswerSet>
    {
        public override void Configure(EntityTypeBuilder<MsAnswerSet> builder)
        {
            builder.Property(x => x.AnswerSetName)
                .IsRequired();

            builder.HasOne(x => x.AcademicYear)
              .WithMany(x => x.AnswerSets)
              .HasForeignKey(fk => fk.IdAcademicYear)
              .HasConstraintName("FK_MsAnswerSet_MsAcademicYear")
              .OnDelete(DeleteBehavior.NoAction)
              .IsRequired();

            base.Configure(builder);
        }
    }
}
