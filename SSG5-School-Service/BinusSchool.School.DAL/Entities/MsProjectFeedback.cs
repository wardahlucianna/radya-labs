using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.Persistence.SchoolDb.Entities.Employee;
using BinusSchool.Persistence.SchoolDb.Entities.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchoolDb.Entities
{
    public class MsProjectFeedback : AuditEntity, ISchoolEntity
    {
        public DateTime RequestDate { get; set; }
        public string IdSchool { get; set; }
        public string IdBinusian { get; set; }
        public string FeatureRequested { get; set; }
        public string Description { get; set; }
        public string IdRelatedModule { get; set; }
        public string IdRelatedSubModule { get; set; }
        public string IdProjectStatus { get; set; }
        public virtual MsSchool School { get; set; }
        public virtual MsStaff Staff { get; set; }
        public virtual MsFeature RelatedModule { get; set; }
        public virtual MsFeature RelatedSubModule { get; set; }
        public virtual LtProjectStatus ProjectStatus { get; set; }
        public virtual ICollection<MsProjectFeedbackSprintMapping> ProjectFeedbackSprintMappings { get; set; }
    }

    internal class MsProjectFeedbackConfiguration : AuditEntityConfiguration<MsProjectFeedback>
    {
        public override void Configure(EntityTypeBuilder<MsProjectFeedback> builder)
        {
            builder.Property(x => x.IdSchool)
                .HasMaxLength(36);

            builder.Property(x => x.IdBinusian)
                .HasMaxLength(36);

            builder.Property(x => x.FeatureRequested)
                .HasMaxLength(1000)
                .IsRequired();

            builder.Property(x => x.Description)
                .HasMaxLength(1000);

            builder.Property(x => x.IdRelatedModule)
                .HasMaxLength(36);

            builder.Property(x => x.IdRelatedSubModule)
                .HasMaxLength(36);

            builder.Property(x => x.IdProjectStatus)
                .HasMaxLength(36);

            builder.HasOne(x => x.School)
                .WithMany(x => x.ProjectFeedbacks)
                .HasForeignKey(x => x.IdSchool)
                .HasConstraintName("FK_MsProjectFeedback_MsSchool")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            builder.HasOne(x => x.Staff)
                .WithMany(x => x.ProjectFeedbacks)
                .HasForeignKey(x => x.IdBinusian)
                .HasConstraintName("FK_MsProjectFeedback_MsStaff")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            builder.HasOne(x => x.RelatedModule)
                .WithMany(x => x.ProjectFeedbacks)
                .HasForeignKey(x => x.IdRelatedModule)
                .HasConstraintName("FK_MsProjectFeedback_MsRelatedModule")
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.RelatedSubModule)
                .WithMany(x => x.SubProjectFeedbacks)
                .HasForeignKey(x => x.IdRelatedSubModule)
                .HasConstraintName("FK_MsProjectFeedback_MsRelatedSubModule")
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.ProjectStatus)
                .WithMany(x => x.ProjectFeedbacks)
                .HasForeignKey(x => x.IdProjectStatus)
                .HasConstraintName("FK_MsProjectFeedback_LtProjectStatus")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
