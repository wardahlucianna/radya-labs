using System;
using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities.School;
using BinusSchool.Persistence.StudentDb.Entities.Scheduling;
using BinusSchool.Persistence.StudentDb.Entities.User;
using BinusSchool.Common.Model.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities
{
    public class TrEvidenceLearning : AuditEntity, IStudentEntity
    {
        public string IdEvidences { get; set; }
        public string IdLearningOutcome { get; set; }
        public virtual TrEvidences Evidences { get; set; }
        public virtual MsLearningOutcome LearningOutcome { get; set; }
    }

    internal class TrEvidenceLearningConfiguration : AuditEntityConfiguration<TrEvidenceLearning>
    {
        public override void Configure(EntityTypeBuilder<TrEvidenceLearning> builder)
        {
            builder.Property(x => x.IdEvidences)
                .HasMaxLength(36)
                .IsRequired();

            builder.Property(x => x.IdLearningOutcome)
                .HasMaxLength(36)
                .IsRequired();

            builder.HasOne(x => x.Evidences)
                .WithMany(x => x.TrEvidenceLearnings)
                .IsRequired()
                .HasForeignKey(fk => fk.IdEvidences)
                .HasConstraintName("FK_TrEvidenceLearning_TrEvidences")
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.LearningOutcome)
                .WithMany(x => x.TrEvidenceLearnings)
                .IsRequired()
                .HasForeignKey(fk => fk.IdLearningOutcome)
                .HasConstraintName("FK_TrEvidenceLearning_MsLearningOutcome")
                .OnDelete(DeleteBehavior.NoAction);

            base.Configure(builder);
        }
    }
}
