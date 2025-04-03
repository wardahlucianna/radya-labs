using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchoolDb.Entities
{
    public class MsProjectPipeline : AuditEntity, ISchoolEntity
    {
        public string Year { get; set; }
        public string IdProjectSection { get; set; }
        public string SprintName { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string IdProjectStatus { get; set; }
        public string IdProjectPhase { get; set; }
        public virtual LtProjectSection ProjectSection { get; set; }
        public virtual LtProjectStatus ProjectStatus { get; set; }
        public virtual LtProjectPhase ProjectPhase { get; set; }
        public virtual ICollection<MsProjectFeedbackSprintMapping> ProjectFeedbackSprintMappings { get; set; }
    }

    internal class MsProjectPipelineConfiguration : AuditEntityConfiguration<MsProjectPipeline>
    {
        public override void Configure(EntityTypeBuilder<MsProjectPipeline> builder)
        {
            builder.Property(x => x.Year)
                .HasMaxLength(4)
                .IsRequired();

            builder.Property(x => x.IdProjectSection)
                .HasMaxLength(36);

            builder.Property(x => x.SprintName)
                .HasMaxLength(1000)
                .IsRequired();

            builder.Property(x => x.Description)
                .HasMaxLength(1000);

            builder.Property(x => x.IdProjectStatus)
                .HasMaxLength(36);

            builder.Property(x => x.IdProjectPhase)
                .HasMaxLength(36);

            builder.HasOne(x => x.ProjectSection)
                .WithMany(x => x.ProjectPipelines)
                .HasForeignKey(x => x.IdProjectSection)
                .HasConstraintName("FK_MsProjectPipeline_LtProjectSection")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            builder.HasOne(x => x.ProjectStatus)
                .WithMany(x => x.ProjectPipelines)
                .HasForeignKey(x => x.IdProjectStatus)
                .HasConstraintName("FK_MsProjectPipeline_LtProjectStatus")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            builder.HasOne(x => x.ProjectPhase)
                .WithMany(x => x.ProjectPipelines)
                .HasForeignKey(x => x.IdProjectPhase)
                .HasConstraintName("FK_MsProjectPipeline_LtProjectPhase")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
