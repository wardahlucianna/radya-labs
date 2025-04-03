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
    public class TrInvitationBookingSettingDetail : AuditEntity, ISchedulingEntity
    {
        public string IdLevel { get; set; }
        public string IdGrade { get; set; }
        public string IdHomeroom { get; set; }
        public string IdInvitationBookingSetting { get; set; }
        public virtual MsLevel Level { get; set; }
        public virtual MsGrade Grade { get; set; }
        public virtual MsHomeroom Homeroom { get; set; }
        public virtual TrInvitationBookingSetting InvitationBookingSetting { get; set; }
    }

    internal class TrInvitationBookingSettingDetailConfiguration : AuditEntityConfiguration<TrInvitationBookingSettingDetail>
    {
        public override void Configure(EntityTypeBuilder<TrInvitationBookingSettingDetail> builder)
        {
            builder.Property(x => x.IdLevel)
                .HasMaxLength(36)
                .IsRequired();

            builder.Property(x => x.IdGrade)
                .HasMaxLength(36);

            builder.Property(x => x.IdHomeroom)
                .HasMaxLength(36);

            builder.Property(x => x.IdInvitationBookingSetting)
                .HasMaxLength(36)
                .IsRequired();

            builder.HasOne(x => x.Level)
                .WithMany(x => x.InvitationBookingSettingDetails)
                .HasForeignKey(fk => fk.IdLevel)
                .HasConstraintName("FK_TrInvitationBookingSettingDetail_MsLevel")
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            builder.HasOne(x => x.Grade)
                .WithMany(x => x.InvitationBookingSettingDetails)
                .HasForeignKey(fk => fk.IdGrade)
                .HasConstraintName("FK_TrInvitationBookingSettingDetail_MsGrade")
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Homeroom)
                .WithMany(x => x.InvitationBookingSettingDetails)
                .HasForeignKey(fk => fk.IdHomeroom)
                .HasConstraintName("FK_TrInvitationBookingSettingDetail_MsHomeroom")
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.InvitationBookingSetting)
               .WithMany(x => x.InvitationBookingSettingDetails)
               .HasForeignKey(fk => fk.IdInvitationBookingSetting)
               .HasConstraintName("FK_TrInvitationBookingSettingDetail_TrInvitationBookingSetting")
               .OnDelete(DeleteBehavior.Restrict)
               .IsRequired();

            base.Configure(builder);
        }
    }
}
