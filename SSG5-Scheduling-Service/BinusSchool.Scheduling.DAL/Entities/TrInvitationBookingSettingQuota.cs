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
    public class TrInvitationBookingSettingQuota : AuditEntity, ISchedulingEntity
    {
        public string IdGrade { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public int? BreakBetweenSession { get; set; }
        public int QuotaSlot { get; set; }
        public int Duration { get; set; }
        public int SettingType { get; set; }
        public DateTime DateInvitation { get; set; }
        public string IdInvitationBookingSetting { get; set; }
        public virtual MsGrade Grade { get; set; }
        public virtual TrInvitationBookingSetting InvitationBookingSetting { get; set; }
    }

    internal class TrInvitationBookingSettingQuotaConfiguration : AuditEntityConfiguration<TrInvitationBookingSettingQuota>
    {
        public override void Configure(EntityTypeBuilder<TrInvitationBookingSettingQuota> builder)
        {

            builder.HasOne(x => x.Grade)
                .WithMany(x => x.InvitationBookingSettingQuotas)
                .HasForeignKey(fk => fk.IdGrade)
                .HasConstraintName("FK_TrInvitationBookingSettingQuota_MsGrade")
                .OnDelete(DeleteBehavior.Restrict);

            builder.Property(x => x.IdInvitationBookingSetting)
                .HasMaxLength(36)
                .IsRequired();

            builder.HasOne(x => x.InvitationBookingSetting)
               .WithMany(x => x.InvitationBookingSettingQuotas)
               .HasForeignKey(fk => fk.IdInvitationBookingSetting)
               .HasConstraintName("FK_TrInvitationBookingSettingQuota_TrInvitationBookingSetting")
               .OnDelete(DeleteBehavior.Restrict)
               .IsRequired();

            base.Configure(builder);
        }
    }
}
