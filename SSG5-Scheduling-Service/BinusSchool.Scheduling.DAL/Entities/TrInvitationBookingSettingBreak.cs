using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchedulingDb.Entities
{
    public class TrInvitationBookingSettingBreak : AuditEntity, ISchedulingEntity
    {
        public string BreakName { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public BreakType BreakType { get; set; }
        public DateTime DateInvitation { get; set; }
        public string IdInvitationBookingSetting { get; set; }
        public string IdGrade { get; set; }
        public virtual TrInvitationBookingSetting InvitationBookingSetting { get; set; }
        public virtual MsGrade Grade { get; set; }
    }

    internal class TrInvitationBookingSettingBreakConfiguration : AuditEntityConfiguration<TrInvitationBookingSettingBreak>
    {
        public override void Configure(EntityTypeBuilder<TrInvitationBookingSettingBreak> builder)
        {
            builder.HasOne(x => x.InvitationBookingSetting)
               .WithMany(x => x.InvitationBookingSettingBreaks)
               .HasForeignKey(fk => fk.IdInvitationBookingSetting)
               .HasConstraintName("FK_TrInvitationBookingSettingBreak_TrInvitationBookingSetting")
               .OnDelete(DeleteBehavior.Restrict)
               .IsRequired();

            builder.HasOne(x => x.Grade)
               .WithMany(x => x.InvitationBookingSettingBreaks)
               .HasForeignKey(fk => fk.IdGrade)
               .HasConstraintName("FK_TrInvitationBookingSettingBreak_MsGrade")
               .OnDelete(DeleteBehavior.Restrict)
               .IsRequired(false);

            builder.Property(e => e.BreakType)
               .HasMaxLength(maxLength: 20)
               .HasConversion(valueToDb =>
               valueToDb.ToString(),
               valueFromDb => (BreakType)Enum.Parse(typeof(BreakType), valueFromDb))
               .IsRequired();

            base.Configure(builder);
        }
    }
}
