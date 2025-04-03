using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities.School;
using BinusSchool.Persistence.AttendanceDb.Entities.User;
using BinusSchool.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.AttendanceDb.Entities
{
    public class MsBlockingTypeAtdSetting : AuditEntity, IAttendanceEntity
    {
        public string IdBlockingType { get; set; }
        public string IdAcademicYear { get; set; }
        public string IdLevel { get; set; }
        public string IdAtdMappingAtd { get; set; }

        public virtual MsBlockingType BlockingType { get; set; }
        public virtual MsAcademicYear AcademicYear { get; set; }
        public virtual MsLevel Level { get; set; }
        public virtual MsAttendanceMappingAttendance AtdMappingAtd { get; set; }


        internal class MsBlockingTypeAtdSettingConfiguration : AuditEntityConfiguration<MsBlockingTypeAtdSetting>
        {
            public override void Configure(EntityTypeBuilder<MsBlockingTypeAtdSetting> builder)
            {
                builder.HasOne(x => x.BlockingType)
                    .WithMany(x => x.BlockingTypeAtdSetting)
                    .HasForeignKey(fk => fk.IdBlockingType)
                    .HasConstraintName("FK_MsBlockingTypeAtdSetting_MsBlockingType")
                    .OnDelete(DeleteBehavior.NoAction)
                    .IsRequired();

                builder.HasOne(x => x.AcademicYear)
                    .WithMany(x => x.BlockingTypeAtdSetting)
                    .HasForeignKey(fk => fk.IdAcademicYear)
                    .HasConstraintName("FK_MsBlockingTypeAtdSetting_MsAcademicYear")
                    .OnDelete(DeleteBehavior.NoAction)
                    .IsRequired();

                builder.HasOne(x => x.Level)
                    .WithMany(x => x.BlockingTypeAtdSetting)
                    .HasForeignKey(fk => fk.IdLevel)
                    .HasConstraintName("FK_MsBlockingTypeAtdSetting_MsLevel")
                    .OnDelete(DeleteBehavior.NoAction)
                    .IsRequired();

                builder.HasOne(x => x.AtdMappingAtd)
                    .WithMany(x => x.BlockingTypeAtdSetting)
                    .HasForeignKey(fk => fk.IdAtdMappingAtd)
                    .HasConstraintName("FK_MsBlockingTypeAtdSetting_MsMappingAttendance")
                    .OnDelete(DeleteBehavior.NoAction)
                    .IsRequired();

                base.Configure(builder);
            }
        }

    }
}
