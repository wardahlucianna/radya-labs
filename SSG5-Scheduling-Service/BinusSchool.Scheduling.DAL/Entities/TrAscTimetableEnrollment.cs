using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchedulingDb.Entities
{
    public class TrAscTimetableEnrollment : AuditEntity, ISchedulingEntity
    {
        public string IdHomeroomStudentEnrollment { get; set; }
        public string IdAscTimetable { get; set; }

        public virtual MsHomeroomStudentEnrollment HomeroomStudentEnrollment { get; set; }
        public virtual TrAscTimetable AscTimetable { get; set; }
    }

    internal class TrAscTimetableEnrollmentConfiguration : AuditEntityConfiguration<TrAscTimetableEnrollment>
    {
        public override void Configure(EntityTypeBuilder<TrAscTimetableEnrollment> builder)
        {
            builder.HasOne(x => x.AscTimetable)
                .WithMany(x => x.AscTimetableEnrollments)
                .HasForeignKey(fk => fk.IdAscTimetable)
                .HasConstraintName("FK_TrAscTimetableEnrollment_TrAscTimetable")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();


            builder.HasOne(x => x.HomeroomStudentEnrollment)
                .WithMany(x => x.AscTimetableEnrollments)
                .HasForeignKey(fk => fk.IdHomeroomStudentEnrollment)
                .HasConstraintName("FK_TrAscTimetableEnrollment_MsHomeroomStudentEnrollment")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
