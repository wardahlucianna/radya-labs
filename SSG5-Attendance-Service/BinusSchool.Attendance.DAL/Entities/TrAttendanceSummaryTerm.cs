using System;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities.Scheduling;
using BinusSchool.Persistence.AttendanceDb.Entities.School;
using BinusSchool.Persistence.AttendanceDb.Entities.Student;
using BinusSchool.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.AttendanceDb.Entities
{
    public class TrAttendanceSummaryTerm : AuditEntity, IAttendanceEntity
    {
        public string IdStudent { get; set; }
        public string IdPeriod { get; set; }
        public string IdAcademicYear { get; set; }
        public string IdSchool { get; set; }
        public string IdLevel { get; set; }
        public string IdGrade { get; set; }
        public string IdHomeroom { get; set; }
        public TrAttendanceSummaryTermType AttendanceWorkhabitType { get; set; }
        public string IdAttendanceWorkhabit { get; set; }
        public string AttendanceWorkhabitName { get; set; }
        public string Message { get; set; }
        public int Total { get; set; }
        public int? TotalInDays { get; set; }
        public int Semester { get; set; }
        public int Term { get; set; }
        public string SourceFileName { get; set; }
        public virtual MsStudent Student { get; set; }
        public virtual MsPeriod Period { get; set; }
        public virtual MsAcademicYear AcademicYear { get; set; }
        public virtual MsSchool School { get; set; }
        public virtual MsLevel Level { get; set; }
        public virtual MsGrade Grade { get; set; }
        public virtual MsHomeroom Homeroom { get; set; }
    }

    internal class TrAttendanceSummaryTermConfiguration : AuditEntityConfiguration<TrAttendanceSummaryTerm>
    {
        public override void Configure(EntityTypeBuilder<TrAttendanceSummaryTerm> builder)
        {
            builder.Property(x => x.IdAttendanceWorkhabit)
                .HasMaxLength(36);

            builder.Property(x => x.AttendanceWorkhabitName)
                .HasMaxLength(36)
                .IsRequired();

            builder.Property(x => x.SourceFileName)
               .HasMaxLength(256);

            builder.Property(e => e.AttendanceWorkhabitType).HasMaxLength(maxLength: 50)
                .HasConversion(valueToDb =>
                        valueToDb.ToString(),
                    valueFromDb =>
                        (TrAttendanceSummaryTermType)Enum.Parse(typeof(TrAttendanceSummaryTermType), valueFromDb));

            builder.HasOne(x => x.Student)
                .WithMany(x => x.AttendanceSummaryTerms)
                .HasForeignKey(fk => fk.IdStudent)
                .HasConstraintName("FK_TrAttendanceSummaryTerm_MsStudent")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder.HasOne(x => x.Period)
                .WithMany(x => x.AttendanceSummaryTerms)
                .HasForeignKey(fk => fk.IdPeriod)
                .HasConstraintName("FK_TrAttendanceSummaryTerm_MsPeriod")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder.HasOne(x => x.AcademicYear)
                .WithMany(x => x.AttendanceSummaryTerms)
                .HasForeignKey(fk => fk.IdAcademicYear)
                .HasConstraintName("FK_TrAttendanceSummaryTerm_MsAcademicYear")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder.HasOne(x => x.School)
                .WithMany(x => x.AttendanceSummaryTerms)
                .HasForeignKey(fk => fk.IdSchool)
                .HasConstraintName("FK_TrAttendanceSummaryTerm_MsSchool")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder.HasOne(x => x.Level)
                .WithMany(x => x.AttendanceSummaryTerms)
                .HasForeignKey(fk => fk.IdLevel)
                .HasConstraintName("FK_TrAttendanceSummaryTerm_MsLevel")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder.HasOne(x => x.Grade)
                .WithMany(x => x.AttendanceSummaryTerms)
                .HasForeignKey(fk => fk.IdGrade)
                .HasConstraintName("FK_TrAttendanceSummaryTerm_MsGrade")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder.HasOne(x => x.Homeroom)
                .WithMany(x => x.AttendanceSummaryTerms)
                .HasForeignKey(fk => fk.IdHomeroom)
                .HasConstraintName("FK_TrAttendanceSummaryTerm_MsHomeroom")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
