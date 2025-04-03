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
    public class TrExperience : AuditEntity, IStudentEntity
    {
        public string IdAcademicYear { get; set; }
        public string IdHomeroomStudent { get; set; }
        public string ExperienceName { get; set; }
        public ExperienceLocation ExperienceLocation { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string SupervisorName { get; set; }
        public string SupervisorTitle { get; set; }
        public string SupervisorEmail { get; set; }
        public string SupervisorContact { get; set; }
        public string IdUserSupervisor { get; set; }
        public string RoleName { get; set; }
        public string PositionName { get; set; }
        public string Organizer { get; set; }
        public string Description { get; set; }
        public string ContributionOrganizer { get; set; }
        public ExperienceStatus Status { get; set; }
        public virtual MsAcademicYear AcademicYear { get; set; }
        public virtual MsHomeroomStudent HomeroomStudent { get; set; }
        public virtual MsUser User { get; set; }
        public virtual ICollection<TrExperienceType> TrExperienceTypes { get; set; }
        public virtual ICollection<TrExperienceLearning> TrExperienceLearnings { get; set; }
        public virtual ICollection<TrEvidences> TrEvidences { get; set; }
        public virtual ICollection<TrExperienceStatusChangeHs> TrExperienceStatusChangeHs { get; set; }
    }

    internal class TrExperienceConfiguration : AuditEntityConfiguration<TrExperience>
    {
        public override void Configure(EntityTypeBuilder<TrExperience> builder)
        {
            builder.Property(x => x.IdAcademicYear)
                .HasMaxLength(36)
                .IsRequired();

            builder.Property(x => x.IdHomeroomStudent)
                .HasMaxLength(36)
                .IsRequired();

            builder.Property(x => x.ExperienceName)
                .HasMaxLength(256)
                .IsRequired();

            builder.Property(x => x.SupervisorName)
                .HasMaxLength(256)
                .IsRequired();

            builder.Property(x => x.SupervisorTitle)
                .HasMaxLength(256)
                .IsRequired();

            builder.Property(x => x.SupervisorEmail)
                .HasMaxLength(256)
                .IsRequired();

            builder.Property(x => x.SupervisorContact)
                .HasMaxLength(50);

            builder.Property(x => x.RoleName)
                .HasMaxLength(256);

            builder.Property(x => x.PositionName)
                .HasMaxLength(256);

            builder.Property(x => x.Organizer)
                .HasMaxLength(256)
                .IsRequired();

            builder.Property(x => x.Description)
                .IsRequired();

            builder.Property(x => x.ContributionOrganizer)
                .IsRequired();

            builder.Property(x => x.ExperienceLocation)
                .HasMaxLength(maxLength: 20)
                .HasConversion(valueToDb =>
                valueToDb.ToString(),
                valueFromDb => (ExperienceLocation)Enum.Parse(typeof(ExperienceLocation), valueFromDb))
                .IsRequired();

            builder.Property(x => x.Status)
                .HasMaxLength(maxLength: 50)
                .HasConversion(valueToDb =>
                valueToDb.ToString(),
                valueFromDb => (ExperienceStatus)Enum.Parse(typeof(ExperienceStatus), valueFromDb))
                .IsRequired();

            builder.HasOne(x => x.AcademicYear)
                .WithMany(x => x.TrExperiences)
                .IsRequired()
                .HasForeignKey(fk => fk.IdAcademicYear)
                .HasConstraintName("FK_TrExperience_MsAcademicYear")
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.HomeroomStudent)
                .WithMany(x => x.TrExperiences)
                .IsRequired()
                .HasForeignKey(fk => fk.IdHomeroomStudent)
                .HasConstraintName("FK_TrExperience_MsHomeroomStudent")
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.User)
                .WithMany(x => x.TrExperiences)
                //.IsRequired()
                .HasForeignKey(fk => fk.IdUserSupervisor)
                .HasConstraintName("FK_TrExperience_MsUser")
                .OnDelete(DeleteBehavior.Cascade);

            base.Configure(builder);
        }
    }
}
