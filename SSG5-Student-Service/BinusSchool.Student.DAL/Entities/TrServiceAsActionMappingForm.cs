using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities
{
    public class TrServiceAsActionMappingForm : AuditNoUniqueEntity, IStudentEntity
    {
        public string IdSAMappingForm { get; set; }
        public string IdMappingLearningOutcome { get; set; }
        public string IdServiceAsActionForm { get; set; }

        public virtual MsMappingLearningOutcome MappingLearningOutcome { get; set; }
        public virtual TrServiceAsActionForm ServiceAsActionForm { get; set; }
    }

    public class TrServiceAsActionMappingFormConfiguration : AuditNoUniqueEntityConfiguration<TrServiceAsActionMappingForm>
    {
        public override void Configure(EntityTypeBuilder<TrServiceAsActionMappingForm> builder)
        {
            builder.HasKey(x => x.IdSAMappingForm);
            builder.Property(x => x.IdSAMappingForm).IsRequired().HasMaxLength(36);
            builder.Property(x => x.IdMappingLearningOutcome).IsRequired().HasMaxLength(36);
            builder.Property(x => x.IdServiceAsActionForm).IsRequired().HasMaxLength(36);

            builder.HasOne(x => x.MappingLearningOutcome)
                .WithMany(x => x.ServiceAsActionMappingForms)
                .HasForeignKey(fk => fk.IdMappingLearningOutcome)
                .HasConstraintName("FK_TrServiceAsActionMappingForm_MsMappingLearningOutcome")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder.HasOne(x => x.ServiceAsActionForm)
                .WithMany(x => x.LOMappings)
                .HasForeignKey(fk => fk.IdServiceAsActionForm)
                .HasConstraintName("FK_TrServiceAsActionMappingForm_TrServiceAsActionForm")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
