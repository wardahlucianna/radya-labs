using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using BinusSchool.Persistence.SchedulingDb.Entities.Teaching;
using BinusSchool.Persistence.SchedulingDb.Entities.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchedulingDb.Entities
{
    public class TrInvitationBookingSettingVenueDtl : AuditEntity, ISchedulingEntity
    {
        public string IdUserTeacher { get; set; }
        public string IdInvitationBookingSettingVenueDate { get; set; }
        public string IdVenue { get; set; }
        public string IdRole { get; set; }
        public string IdTeacherPosition { get; set; }
        public virtual MsUser User { get; set; }
        public virtual MsVenue Venue { get; set; }
        public virtual TrInvitationBookingSettingVenueDate InvitationBookingSettingVenueDate { get; set; }
        public virtual MsTeacherPosition TeacherPosition { get; set; }
        public virtual LtRole Role { get; set; }
    }

    internal class TrInvitationBookingSettingVenueDtlConfiguration : AuditEntityConfiguration<TrInvitationBookingSettingVenueDtl>
    {
        public override void Configure(EntityTypeBuilder<TrInvitationBookingSettingVenueDtl> builder)
        {
            builder.Property(x => x.IdUserTeacher)
               .HasMaxLength(36)
               .IsRequired();

            builder.Property(x => x.IdInvitationBookingSettingVenueDate)
               .HasMaxLength(36)
               .IsRequired();

            builder.Property(x => x.IdVenue)
               .HasMaxLength(36)
               .IsRequired();

            builder.Property(x => x.IdRole)
               .HasMaxLength(36)
               //.IsRequired()
               ;

            builder.Property(x => x.IdTeacherPosition)
               .HasMaxLength(36)
               //.IsRequired()
               ;

            builder.HasOne(x => x.User)
               .WithMany(x => x.InvitationBookingSettingVenueDtls)
               .HasForeignKey(fk => fk.IdUserTeacher)
               .HasConstraintName("FK_TrInvitationBookingSettingVenueDtl_MsUser")
               .OnDelete(DeleteBehavior.Restrict)
               .IsRequired();

            builder.HasOne(x => x.Venue)
               .WithMany(x => x.InvitationBookingSettingVenueDtls)
               .HasForeignKey(fk => fk.IdVenue)
               .HasConstraintName("FK_TrInvitationBookingSettingVenueDtl_MsVenue")
               .OnDelete(DeleteBehavior.Restrict)
               .IsRequired();

            builder.HasOne(x => x.InvitationBookingSettingVenueDate)
               .WithMany(x => x.InvitationBookingSettingVenueDtl)
               .HasForeignKey(fk => fk.IdInvitationBookingSettingVenueDate)
               .HasConstraintName("FK_TrInvitationBookingSettingVenueDtl_TrInvitationBookingVenueDate")
               .OnDelete(DeleteBehavior.Restrict)
               .IsRequired();

            builder.HasOne(x => x.Role)
               .WithMany(x => x.InvitationBookingSettingVenueDtl)
               .HasForeignKey(fk => fk.IdRole)
               .HasConstraintName("FK_TrInvitationBookingSettingVenueDtl_LtRole")
               .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.TeacherPosition)
               .WithMany(x => x.InvitationBookingSettingVenueDtl)
               .HasForeignKey(fk => fk.IdTeacherPosition)
               .HasConstraintName("FK_TrInvitationBookingSettingVenueDtl_MsTeacherPosition")
               .OnDelete(DeleteBehavior.Restrict);

            base.Configure(builder);
        }
    }
}
