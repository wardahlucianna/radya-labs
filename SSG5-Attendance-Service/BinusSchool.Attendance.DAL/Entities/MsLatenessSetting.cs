using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities.School;
using BinusSchool.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.AttendanceDb.Entities
{
    public class MsLatenessSetting : AuditEntity, IAttendanceEntity
    {
        public string IdLevel { get; set; }
        public PeriodType Period { get; set; }
        public int TotalLate { get; set; }
        public int TotalUnexcusedAbsend { get; set; }
        public virtual MsLevel Level { get; set; }

    }

    internal class MsLatenessSettingConfiguration : AuditEntityConfiguration<MsLatenessSetting>
    {
        public override void Configure(EntityTypeBuilder<MsLatenessSetting> builder)
        {
            builder.HasOne(x => x.Level)
                .WithMany(x => x.LatenessSettings)
                .HasForeignKey(fk => fk.IdLevel)
                .HasConstraintName("FK_MsLatenessSetting_MsLevel")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            builder.Property(e => e.Period).HasMaxLength(maxLength: 10)
               .HasConversion(valueToDb =>
               valueToDb.ToString(),
               valueFromDb => (PeriodType)Enum.Parse(typeof(PeriodType), valueFromDb));

            base.Configure(builder);
        }
    }
}
