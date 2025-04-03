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
    public class TrExperienceLearning : AuditEntity, IStudentEntity
    {
        public string IdExperience { get; set; }
        public string IdLearningOutcome { get; set; }
        public virtual TrExperience Experience { get; set; }
        public virtual MsLearningOutcome LearningOutcome { get; set; }
        // public virtual ICollection<TrExperienceStatusChangeHs> TrExperienceStatusChangeHs { get; set; }
    }

    internal class TrExperienceLearningConfiguration : AuditEntityConfiguration<TrExperienceLearning>
    {
        public override void Configure(EntityTypeBuilder<TrExperienceLearning> builder)
        {
            builder.Property(x => x.IdExperience)
                .HasMaxLength(36)
                .IsRequired();

            builder.Property(x => x.IdLearningOutcome)
                .HasMaxLength(36)
                .IsRequired();

            builder.HasOne(x => x.Experience)
                .WithMany(x => x.TrExperienceLearnings)
                .IsRequired()
                .HasForeignKey(fk => fk.IdExperience)
                .HasConstraintName("FK_TrExperienceLearning_TrExperience")
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.LearningOutcome)
                .WithMany(x => x.TrExperienceLearnings)
                .IsRequired()
                .HasForeignKey(fk => fk.IdLearningOutcome)
                .HasConstraintName("FK_TrExperienceLearning_MsLearningOutcome")
                .OnDelete(DeleteBehavior.NoAction);

            base.Configure(builder);
        }
    }
}
