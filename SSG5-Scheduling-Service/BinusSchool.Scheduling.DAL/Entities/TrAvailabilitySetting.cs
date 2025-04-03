using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using BinusSchool.Persistence.SchedulingDb.Entities.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchedulingDb.Entities
{
    public class TrAvailabilitySetting : AuditEntity, ISchedulingEntity
    {
        public string IdUserTeacher { get; set; }
        public string IdAcademicYear { get; set; }
        public int Semester { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string Day { get; set; }
        public virtual MsUser User { get; set; }
        public virtual MsAcademicYear AcademicYear { get; set; }
    }

    internal class TrAvailabilitySettingConfiguration : AuditEntityConfiguration<TrAvailabilitySetting>
    {
        public override void Configure(EntityTypeBuilder<TrAvailabilitySetting> builder)
        {
            builder.Property(x => x.Day)
                .HasMaxLength(10)
                .IsRequired();

            builder.HasOne(x => x.User)
               .WithMany(x => x.AvailabilitySettings)
               .HasForeignKey(fk => fk.IdUserTeacher)
               .HasConstraintName("FK_TrAvailabilitySetting_MsUser")
               .OnDelete(DeleteBehavior.Restrict)
               .IsRequired();

            builder.HasOne(x => x.AcademicYear)
               .WithMany(x => x.AvailabilitySettings)
               .HasForeignKey(fk => fk.IdAcademicYear)
               .HasConstraintName("FK_TrAvailabilitySetting_MsAcademicYear")
               .OnDelete(DeleteBehavior.Restrict)
               .IsRequired();

            base.Configure(builder);
        }
    }
}
