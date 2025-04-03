using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities.School;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities
{
    public class MsMappingLearningOutcome : AuditEntity, IStudentEntity
    {
        public string IdLearningOutcome { get; set; }
        public string IdAcademicyear { get; set; }

        public virtual MsLearningOutcome LearningOutcome { get; set; }
        public virtual MsAcademicYear AcademicYear { get; set; }

        public virtual ICollection<TrServiceAsActionMapping> EvidenceMappings { get; set; }
        public virtual ICollection<TrServiceAsActionMappingForm> ServiceAsActionMappingForms { get; set; }
    }

    internal class MsMappingLearningOutcomeConfiguration : AuditEntityConfiguration<MsMappingLearningOutcome>
    {
        public override void Configure(EntityTypeBuilder<MsMappingLearningOutcome> builder)
        {
            builder.Property(x => x.IdLearningOutcome).IsRequired().HasMaxLength(36);

            builder.Property(x => x.IdAcademicyear).IsRequired().HasMaxLength(36);

            builder.HasOne(x => x.LearningOutcome)
                .WithMany(x => x.MappingLearningOutcomes)
                .HasForeignKey(fk => fk.IdLearningOutcome)
                .HasConstraintName("FK_MsMappingLearningOutcome_MsLearningOutcome")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder.HasOne(x => x.AcademicYear)
                .WithMany(x => x.MappingLearningOutcomes)
                .HasForeignKey(fk => fk.IdAcademicyear)
                .HasConstraintName("FK_MsMappingLearningOutcome_MsAcademicYear")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
