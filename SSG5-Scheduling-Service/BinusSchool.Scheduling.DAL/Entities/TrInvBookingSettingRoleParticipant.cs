using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities.User;
using BinusSchool.Persistence.SchedulingDb.Entities.Teaching;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchedulingDb.Entities
{
    public class TrInvBookingSettingRoleParticipant : AuditEntity, ISchedulingEntity
    {
        public string IdRole { get; set; }
        public string IdTeacherPosition { get; set; }
        public string IdInvitationBookingSetting { get; set; }
        public virtual LtRole Role { get; set; }
        public virtual MsTeacherPosition TeacherPosition { get; set; }
        public virtual TrInvitationBookingSetting InvitationBookingSetting { get; set; }
    }

    internal class TrInvBookingSettingRoleParticipantConfiguration : AuditEntityConfiguration<TrInvBookingSettingRoleParticipant>
    {
        public override void Configure(EntityTypeBuilder<TrInvBookingSettingRoleParticipant> builder)
        {

            builder.Property(x => x.IdRole)
                .HasMaxLength(36)
                .IsRequired(false);

            builder.Property(x => x.IdTeacherPosition)
                .HasMaxLength(36)
                .IsRequired(false);

            builder.Property(x => x.IdInvitationBookingSetting)
                .HasMaxLength(36)
                .IsRequired();

            builder.HasOne(x => x.Role)
               .WithMany(x => x.InvBookingSettingRoleParticipants)
               .HasForeignKey(fk => fk.IdRole)
               .HasConstraintName("FK_TrInvBookingSettingRoleParticipant_LtRole")
               .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.TeacherPosition)
               .WithMany(x => x.InvBookingSettingRoleParticipants)
               .HasForeignKey(fk => fk.IdTeacherPosition)
               .HasConstraintName("FK_TrInvBookingSettingRoleParticipant_MsTeacherPosition")
               .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.InvitationBookingSetting)
                .WithMany(x => x.InvBookingSettingRoleParticipants)
                .HasForeignKey(fk => fk.IdInvitationBookingSetting)
                .HasConstraintName("FK_TrInvBookingSettingRoleParticipant_TrInvitationBookingSetting")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
