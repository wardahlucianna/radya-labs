using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using BinusSchool.Persistence.SchedulingDb.Entities.User;
using BinusSchool.Persistence.SchedulingDb.Entities.Teaching;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BinusSchool.Persistence.SchedulingDb.Entities.Employee;
using System;

namespace BinusSchool.Persistence.SchedulingDb.Entities
{
    public class MsSettingSchedulePublishDate : AuditEntity, ISchedulingEntity
    {
        public DateTime PublishDate { get; set; }
        public string IdSchool { get; set; }
        public string IdAcademicYear { get; set; }
        public virtual MsSchool School { get; set; }
        public virtual MsAcademicYear MsAcademicYear { get; set; }
    }

    internal class MsSettingSchedulePublishDateConfiguration : AuditEntityConfiguration<MsSettingSchedulePublishDate>
    {
        public override void Configure(EntityTypeBuilder<MsSettingSchedulePublishDate> builder)
        {
            builder.Property(x => x.PublishDate)
                .HasMaxLength(7)
                .IsRequired();

            builder.HasOne(x => x.School)
              .WithMany(x => x.MsSettingSchedulePublishDates)
              .HasForeignKey(fk => fk.IdSchool)
              .HasConstraintName("FK_MsSettingSchedulePublishDates_MsSchool")
              .OnDelete(DeleteBehavior.Restrict)
              .IsRequired();

            builder.HasOne(x => x.MsAcademicYear)
              .WithMany(x => x.MsSettingSchedulePublishDates)
              .HasForeignKey(fk => fk.IdAcademicYear)
              .HasConstraintName("FK_MsSettingSchedulePublishDates_MsAcademicYear")
              .OnDelete(DeleteBehavior.Restrict)
              .IsRequired();

            base.Configure(builder);
        }
    }
}
