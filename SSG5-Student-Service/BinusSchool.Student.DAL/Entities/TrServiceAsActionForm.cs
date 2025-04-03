using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities.User;
using BinusSchool.Student.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities
{
    public class TrServiceAsActionForm : AuditEntity, IStudentEntity
    {
        public string IdServiceAsActionHeader { get; set; }
        public string IdServiceAsActionStatus { get; set; }
        public string? IdSupervisor { get; set; }
        public string IdServiceAsActionLocDesc { get; set; }
        public string ExpName { get; set; }
        public string? SupervisorContact { get; set; }
        public string? SupervisorTitle { get; set; }
        public string? SupervisorName { get; set; }
        public string? SupervisorEmail { get; set; }
        public string OrganizationName { get; set; }
        public string ContributionTMC { get; set; }
        public string ActivityDesc { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? RevisionNote { get; set; }
        public string? ApprovedBy { get; set; }
        public DateTime? ApprovedDate { get; set; }

        public virtual TrServiceAsActionHeader ServiceAsActionHeader { get; set; }
        public virtual MsServiceAsActionStatus ServiceAsActionStatus { get; set; }
        public virtual MsUser? Supervisor { get; set; }
        public virtual MsUser? UserApprove { get; set; }
        public virtual LtServiceAsActionLocDesc ServiceAsActionLocation { get; set; }

        public virtual ICollection<TrServiceAsActionEvidence> ServiceAsActionEvidences { get; set; }
        public virtual ICollection<TrServiceAsActionMappingForm> LOMappings { get; set; }
        public virtual ICollection<TrServiceAsActionMappingType> ServiceAsActionMappingTypes { get; set; }
        public virtual ICollection<TrServiceAsActionMappingSdgs> ServiceAsActionMappingSdgs { get; set; }
    }

    internal class TrExperienceFormConfiguration : AuditEntityConfiguration<TrServiceAsActionForm>
    {
        public override void Configure(EntityTypeBuilder<TrServiceAsActionForm> builder)
        {
            builder.Property(x => x.IdServiceAsActionHeader).IsRequired().HasMaxLength(36);
            builder.Property(x => x.IdServiceAsActionStatus).IsRequired().HasMaxLength(36);
            builder.Property(x => x.IdSupervisor).HasMaxLength(36);
            builder.Property(x => x.SupervisorName).HasMaxLength(100);
            builder.Property(x => x.SupervisorEmail).HasMaxLength(100);
            builder.Property(x => x.IdServiceAsActionLocDesc).IsRequired().HasMaxLength(36);
            builder.Property(x => x.ExpName).IsRequired().HasMaxLength(50);
            builder.Property(x => x.SupervisorContact).HasMaxLength(50);
            builder.Property(x => x.SupervisorTitle).HasMaxLength(50);
            builder.Property(x => x.OrganizationName).IsRequired().HasMaxLength(50);
            builder.Property(x => x.ContributionTMC).IsRequired().HasMaxLength(50);
            builder.Property(x => x.ActivityDesc).IsRequired().HasMaxLength(300);
            builder.Property(x => x.StartDate).IsRequired();
            builder.Property(x => x.EndDate).IsRequired();
            builder.Property(x => x.RevisionNote).HasMaxLength(200);
            builder.Property(x => x.ApprovedBy).HasMaxLength(36);


            builder.HasOne(x => x.ServiceAsActionHeader)
                .WithMany(x => x.ServiceAsActionForms)
                .HasForeignKey(fk => fk.IdServiceAsActionHeader)
                .HasConstraintName("FK_TrExperienceForm_TrExperienceHeader")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder.HasOne(x => x.ServiceAsActionStatus)
                .WithMany(x => x.ServiceAsActionForms)
                .HasForeignKey(fk => fk.IdServiceAsActionStatus)
                .HasConstraintName("FK_TrExperienceForm_MsExperienceStatus")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder.HasOne(x => x.Supervisor)
                .WithMany(x => x.ServiceAsActionForms)
                .HasForeignKey(fk => fk.IdSupervisor)
                .HasConstraintName("FK_TrExperienceForm_MsUser")
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.ServiceAsActionLocation)
                .WithMany(x => x.ServiceAsActionForms)
                .HasForeignKey(fk => fk.IdServiceAsActionLocDesc)
                .HasConstraintName("FK_TrExperienceForm_MsExperienceLocation")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder.HasOne(x => x.UserApprove)
                .WithMany(x => x.ServiceAsActionApproves)
                .HasForeignKey(fk => fk.ApprovedBy)
                .HasConstraintName("FK_TrExperienceForm_MsUser_ApprovedBy")
                .OnDelete(DeleteBehavior.NoAction);

            base.Configure(builder);
        }
    }
}
