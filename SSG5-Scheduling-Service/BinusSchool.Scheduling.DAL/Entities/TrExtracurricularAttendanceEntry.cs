using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities.Student;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchedulingDb.Entities
{
    public class TrExtracurricularAttendanceEntry : AuditEntity, ISchedulingEntity
    {
        public string IdExtracurricularGeneratedAtt { get; set; }
        public string IdStudent { get; set; }
        public string IdExtracurricularStatusAtt { get; set; }
        public string Reason { get; set; }
        public bool? IsPresent { get; set; }

        public virtual TrExtracurricularGeneratedAtt ExtracurricularGeneratedAtt { get; set; }
        public virtual MsStudent Student { get; set; }
        public virtual LtExtracurricularStatusAtt ExtracurricularStatusAtt { get; set; }


    }

    internal class TrExtracurricularAttendanceEntryConfiguration : AuditEntityConfiguration<TrExtracurricularAttendanceEntry>
    {
        public override void Configure(EntityTypeBuilder<TrExtracurricularAttendanceEntry> builder)
        {
            builder.Property(x => x.IdExtracurricularGeneratedAtt)
            .HasMaxLength(36)
            .IsRequired();

            builder.Property(x => x.IdStudent)
            .HasMaxLength(36)
            .IsRequired();

            builder.Property(x => x.IdExtracurricularStatusAtt)
            .HasMaxLength(36)
            .IsRequired();

            builder.Property(x => x.Reason)
            .HasMaxLength(200);

            builder.HasOne(x => x.ExtracurricularGeneratedAtt)
                 .WithMany(y => y.ExtracurricularAttendanceEntries)
                 .HasForeignKey(fk => fk.IdExtracurricularGeneratedAtt)
                 .HasConstraintName("FK_TrExtracurricularAttendanceEntry_TrExtracurricularGeneratedAtt")
                 .OnDelete(DeleteBehavior.Restrict);


            builder.HasOne(x => x.Student)
                 .WithMany(y => y.ExtracurricularAttendanceEntries)
                 .HasForeignKey(fk => fk.IdStudent)
                 .HasConstraintName("FK_TrExtracurricularAttendanceEntry_MsStudent")
                 .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.ExtracurricularStatusAtt)
                 .WithMany(y => y.ExtracurricularAttendanceEntries)
                 .HasForeignKey(fk => fk.IdExtracurricularStatusAtt)
                 .HasConstraintName("FK_TrExtracurricularAttendanceEntry_LtExtracurricularStatusAtt")
                 .OnDelete(DeleteBehavior.Restrict);


            base.Configure(builder);
        }
    }
}
