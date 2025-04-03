using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities.User;
using BinusSchool.Persistence.SchedulingDb.Entities.Teaching;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BinusSchool.Persistence.SchedulingDb.Entities.School;

namespace BinusSchool.Persistence.SchedulingDb.Entities
{
    public class TrInvitationBookingSettingExcludeSub : AuditEntity, ISchedulingEntity
    {
        public string IdGrade { get; set; }
        public string IdSubject { get; set; }
        public string IdInvitationBookingSetting { get; set; }
        public virtual MsSubject Subject { get; set; }
        public virtual MsGrade Grade { get; set; }
        public virtual TrInvitationBookingSetting InvitationBookingSetting { get; set; }
    }

    internal class TrInvitationBookingSettingExcludeSubConfiguration : AuditEntityConfiguration<TrInvitationBookingSettingExcludeSub>
    {
        public override void Configure(EntityTypeBuilder<TrInvitationBookingSettingExcludeSub> builder)
        {

            builder.Property(x => x.IdGrade)
                .HasMaxLength(36)
                .IsRequired(false);

            builder.Property(x => x.IdSubject)
                .HasMaxLength(36)
                .IsRequired(false);

            builder.Property(x => x.IdInvitationBookingSetting)
                .HasMaxLength(36)
                .IsRequired();

            builder.HasOne(x => x.Subject)
               .WithMany(x => x.InvBookingSettingExcludeSub)
               .HasForeignKey(fk => fk.IdSubject)
               .HasConstraintName("FK_TrInvitationBookingSettingExcludeSub_MsSubject")
               .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Grade)
               .WithMany(x => x.InvBookingSettingExcludeSub)
               .HasForeignKey(fk => fk.IdGrade)
               .HasConstraintName("FK_TrInvitationBookingSettingExcludeSub_MsGrade")
               .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.InvitationBookingSetting)
                .WithMany(x => x.InvBookingSettingExcludeSub)
                .HasForeignKey(fk => fk.IdInvitationBookingSetting)
                .HasConstraintName("FK_TrInvitationBookingSettingExcludeSub_TrInvitationBookingSetting")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
