using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using BinusSchool.Persistence.SchedulingDb.Entities.User;
using BinusSchool.Persistence.SchedulingDb.Entities.Student;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchedulingDb.Entities
{
    public class TrPersonalInvitation : AuditEntity, ISchedulingEntity
    {
        public string IdAcademicYear { get; set; }
        public string IdUserInvitation { get; set; }
        public string IdUserTeacher { get; set; }
        public string IdStudent { get; set; }
        public string IdVenue { get; set; }
        public PersonalInvitationType? InvitationType { get; set; }
        public DateTime InvitationDate { get; set; }
        public TimeSpan InvitationStartTime { get; set; }
        public TimeSpan InvitationEndTime { get; set; }
        public PersonalInvitationStatus Status { get; set; }
        public DateTime? AvailabilityDate { get; set; }
        public TimeSpan? AvailabilityStartTime { get; set; }
        public TimeSpan? AvailabilityEndTime { get; set; }
        public DateTime? DateApproval { get; set; }
        public string DeclineReason { get; set; }
        public string Description { get; set; }
        public bool IsStudent { get; set; }
        public bool IsMother { get; set; }
        public bool IsNotifParent { get; set; }

        public bool IsFather { get; set; }
        public bool IsAvailable { get; set; }
        public virtual MsAcademicYear AcademicYear { get; set; }
        public virtual MsUser UserInvitation { get; set; }
        public virtual MsUser UserTeacher { get; set; }
        public virtual MsVenue Venue { get; set; }
        public virtual MsStudent Student { get; set; }
    }

}

internal class TrPersonalInvitationConfiguration : AuditEntityConfiguration<TrPersonalInvitation>
{
    public override void Configure(EntityTypeBuilder<TrPersonalInvitation> builder)
    {
        builder.Property(x => x.DeclineReason)
               .HasConversion<string>()
               .HasMaxLength(450);

        builder.Property(x => x.Description)
               .HasConversion<string>()
               .HasMaxLength(450);

        builder.Property(e => e.Status).HasMaxLength(maxLength: 20)
               .HasConversion(valueToDb =>
               valueToDb.ToString(),
               valueFromDb => (PersonalInvitationStatus)Enum.Parse(typeof(PersonalInvitationStatus), valueFromDb));

        builder.Property(e => e.InvitationType).HasMaxLength(maxLength: 25)
               .HasConversion(valueToDb =>
               valueToDb.ToString(),
               valueFromDb => (PersonalInvitationType)Enum.Parse(typeof(PersonalInvitationType), valueFromDb));

        builder.HasOne(x => x.AcademicYear)
           .WithMany(x => x.PersonalInvitations)
           .HasForeignKey(fk => fk.IdAcademicYear)
           .HasConstraintName("FK_TrPersonalInvitation_MsAcademicYear")
           .OnDelete(DeleteBehavior.NoAction)
            .IsRequired();

        builder.HasOne(x => x.UserInvitation)
               .WithMany(x => x.PersonalInvitations)
               .HasForeignKey(fk => fk.IdUserInvitation)
               .HasConstraintName("FK_TrPersonalInvitation_MsUser")
               .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

        builder.HasOne(x => x.UserTeacher)
               .WithMany(x => x.PersonalInvitationsTeacher)
               .HasForeignKey(fk => fk.IdUserTeacher)
               .HasConstraintName("FK_TrPersonalInvitation_MsUserTeacher")
               .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

        builder.HasOne(x => x.Venue)
              .WithMany(x => x.PersonalInvitations)
              .HasForeignKey(fk => fk.IdVenue)
              .HasConstraintName("FK_TrPersonalInvitation_MsVenue")
              .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(x => x.Student)
              .WithMany(x => x.PersonalInvitations)
              .HasForeignKey(fk => fk.IdStudent)
              .HasConstraintName("FK_TrPersonalInvitation_MsStudent")
              .OnDelete(DeleteBehavior.NoAction)
               .IsRequired();

        base.Configure(builder);
    }
}
