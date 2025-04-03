using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities.Student;
using BinusSchool.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.AttendanceDb.Entities.User
{
    public class HMsStudentBlocking : AuditNoUniqueEntity, IAttendanceEntity
    {
        public string IdHMsStudentBlocking { get; set; }
        public string IdStudent { get; set; }
        public string IdBlockingCategory { get; set; }
        public string IdBlockingType { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public virtual MsStudent Student { get; set; }
        public virtual MsBlockingCategory BlockingCategory { get; set; }
        public virtual MsBlockingType BlockingType { get; set; }
    }

    internal class HMsStudentBlockingConfiguration : AuditNoUniqueEntityConfiguration<HMsStudentBlocking>
    {
        public override void Configure(EntityTypeBuilder<HMsStudentBlocking> builder)
        {
            builder.HasKey(x => x.IdHMsStudentBlocking);

            builder.HasOne(x => x.Student)
                .WithMany(x => x.HistoryStudentBlockings)
                .HasForeignKey(fk => fk.IdStudent)
                .HasConstraintName("FK_HMsStudentBlocking_MsStudent")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder.HasOne(x => x.BlockingCategory)
                .WithMany(x => x.HistoryStudentBlockings)
                .HasForeignKey(fk => fk.IdBlockingCategory)
                .HasConstraintName("FK_HMsStudentBlocking_MsBlockingCategory")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder.HasOne(x => x.BlockingType)
                .WithMany(x => x.HistoryStudentBlockings)
                .HasForeignKey(fk => fk.IdBlockingType)
                .HasConstraintName("FK_HMsStudentBlocking_MsBlockingType")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder.Property(p => p.StartDate)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
