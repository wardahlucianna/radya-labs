using System;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities.School;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Collections.Generic;

namespace BinusSchool.Persistence.AttendanceDb.Entities.School
{
    public class MsPeriod : CodeEntity, IAttendanceEntity
    {
        public string IdGrade { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime AttendanceStartDate { get; set; }
        public DateTime AttendanceEndDate { get; set; }
        public int Semester { get; set; }
        public int OrderNumber { get; set; }
        public virtual MsGrade Grade { get; set; }
        public virtual ICollection<TrAttendanceSummaryTerm> AttendanceSummaryTerms { get; set; }
    }

    internal class MsPeriodConfiguration : CodeEntityConfiguration<MsPeriod>
    {
        public override void Configure(EntityTypeBuilder<MsPeriod> builder)
        {
            builder.Property(x => x.StartDate)
                .IsRequired();
                
            builder.Property(x => x.EndDate)
                .IsRequired();

            builder.Property(x => x.AttendanceStartDate)
                .IsRequired();
                
            builder.Property(x => x.AttendanceEndDate)
                .IsRequired();
                
            builder.Property(x => x.Semester)
                .IsRequired();

            builder.HasOne(x => x.Grade)
                .WithMany(x => x.Periods)
                .HasForeignKey(fk => fk.IdGrade)
                .HasConstraintName("FK_MsPeriod_MsGrade")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();


            base.Configure(builder);
        }
    }
}
