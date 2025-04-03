using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchedulingDb.Entities
{
    public class TrInvitationBookingSettingUser : AuditEntity, ISchedulingEntity
    {
        public string IdHomeroomStudent { get; set; }
        public string IdInvitationBookingSetting { get; set; }
        public virtual MsHomeroomStudent HomeroomStudent { get; set; }
        public virtual TrInvitationBookingSetting InvitationBookingSetting { get; set; }
    }

    internal class TrInvitationBookingSettingUserConfiguration : AuditEntityConfiguration<TrInvitationBookingSettingUser>
    {
        public override void Configure(EntityTypeBuilder<TrInvitationBookingSettingUser> builder)
        {

            builder.Property(x => x.IdHomeroomStudent)
                .HasMaxLength(36)
                .IsRequired();

            builder.Property(x => x.IdInvitationBookingSetting)
                .HasMaxLength(36)
                .IsRequired();

            builder.HasOne(x => x.HomeroomStudent)
                .WithMany(x => x.InvitationBookingSettingUsers)
                .HasForeignKey(fk => fk.IdHomeroomStudent)
                .HasConstraintName("FK_TrInvitationBookingSettingUser_MsHomeroomStudent")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            builder.HasOne(x => x.InvitationBookingSetting)
                .WithMany(x => x.InvitationBookingSettingUsers)
                .HasForeignKey(fk => fk.IdInvitationBookingSetting)
                .HasConstraintName("FK_TrInvitationBookingSettingUser_TrInvitationBookingSetting")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
