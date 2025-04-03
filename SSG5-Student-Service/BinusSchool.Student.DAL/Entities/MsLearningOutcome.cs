using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities.School;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities
{
    public class MsLearningOutcome : AuditEntity, IStudentEntity
    {
        public string IdAcademicYear { get; set; }
        public string LearningOutcomeName { get; set; }
        public int Order { get; set; }
        public virtual MsAcademicYear AcademicYear { get; set; }
        public virtual ICollection<TrExperienceLearning> TrExperienceLearnings { get; set; }

        public virtual ICollection<TrEvidenceLearning> TrEvidenceLearnings { get; set; }
        public virtual ICollection<MsMappingLearningOutcome> MappingLearningOutcomes { get; set; }
    }

    internal class MsLearningOutcomeConfiguration : AuditEntityConfiguration<MsLearningOutcome>
    {
        public override void Configure(EntityTypeBuilder<MsLearningOutcome> builder)
        {
            builder.Property(x => x.IdAcademicYear)
                .HasMaxLength(36)
                .IsRequired();

            builder.Property(x => x.LearningOutcomeName)
                .HasMaxLength(256)
                .IsRequired();

            builder.HasOne(x => x.AcademicYear)
                .WithMany(x => x.LearningOutcomes)
                .IsRequired()
                .HasForeignKey(fk => fk.IdAcademicYear)
                .HasConstraintName("FK_MsLearningOutcome_MsAcademicYear")
                .OnDelete(DeleteBehavior.NoAction);

            base.Configure(builder);
        }
    }
}
