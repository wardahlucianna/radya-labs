using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchoolDb.Entities
{
    public class MsProjectFeedbackSprintMapping : AuditEntity, ISchoolEntity
    {
        public string IdProjectFeedback { get; set; }
        public string IdProjectPipeline { get; set; }
        public virtual MsProjectFeedback ProjectFeedback { get; set; }
        public virtual MsProjectPipeline ProjectPipeline { get; set; }
    }

    internal class MsProjectFeedbackSprintMappingConfiguration : AuditEntityConfiguration<MsProjectFeedbackSprintMapping>
    {
        public override void Configure(EntityTypeBuilder<MsProjectFeedbackSprintMapping> builder)
        {
            builder.Property(x => x.IdProjectFeedback)
                .HasMaxLength(36);

            builder.Property(x => x.IdProjectPipeline)
                .HasMaxLength(36);

            builder.HasOne(x => x.ProjectFeedback)
                .WithMany(x => x.ProjectFeedbackSprintMappings)
                .HasForeignKey(x => x.IdProjectFeedback)
                .HasConstraintName("FK_MsProjectFeedbackSprintMapping_MsProjectFeedback")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            builder.HasOne(x => x.ProjectPipeline)
                .WithMany(x => x.ProjectFeedbackSprintMappings)
                .HasForeignKey(x => x.IdProjectPipeline)
                .HasConstraintName("FK_MsProjectFeedbackSprintMapping_MsProjectPipeline")
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
