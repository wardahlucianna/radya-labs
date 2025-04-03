using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities.Scheduling;
using BinusSchool.Persistence.StudentDb.Entities.School;
using BinusSchool.Persistence.StudentDb.Entities.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities
{
    public class TrGcReportStudent : AuditEntity, IStudentEntity
    {
        public string IdAcademicYear { get; set; }
        public string IdStudent { get; set; }
        public string IdUserCounselor { get; set; }
        public string IdUserReport { get; set; }
        public DateTime Date { get; set; }
        public string Note { get; set; }
        public bool IsRead { get; set; }
        public string IdGcReportStudentGrade { get; set; }
        public virtual MsAcademicYear AcademicYear { get; set; }
        public virtual MsStudent Student { get; set; }
        public virtual MsUser UserConsellor { get; set; }
        public virtual MsUser UserReport { get; set; }
        public virtual TrGcReportStudentGrade GcReportStudentGrade { get; set; }

    }

    internal class TrGcReportStudentConfiguration : AuditEntityConfiguration<TrGcReportStudent>
    {
        public override void Configure(EntityTypeBuilder<TrGcReportStudent> builder)
        {
            builder.Property(x => x.Date).IsRequired();
            builder.Property(x => x.Note).HasMaxLength(450).IsRequired();

            builder.HasOne(x => x.AcademicYear)
             .WithMany(x => x.GcReportStudent)
             .HasForeignKey(fk => fk.IdAcademicYear)
             .HasConstraintName("FK_TrGcReportStudent_MsAcademicYear")
             .OnDelete(DeleteBehavior.Restrict)
             .IsRequired();


            builder.HasOne(x => x.Student)
             .WithMany(x => x.GcReportStudent)
             .HasForeignKey(fk => fk.IdStudent)
             .HasConstraintName("FK_TrGcReportStudent_MsStudent")
             .OnDelete(DeleteBehavior.Restrict)
             .IsRequired();

            builder.HasOne(x => x.UserConsellor)
             .WithMany(x => x.GcReportStudentUserConsoller)
             .HasForeignKey(fk => fk.IdUserCounselor)
             .HasConstraintName("FK_TrGcReportStudent_MsUserConsoller")
             .OnDelete(DeleteBehavior.Restrict)
             .IsRequired();

            builder.HasOne(x => x.UserReport)
             .WithMany(x => x.GcReportStudentUserReport)
             .HasForeignKey(fk => fk.IdUserReport)
             .HasConstraintName("FK_TrGcReportStudent_MsUserReport")
             .OnDelete(DeleteBehavior.Restrict)
             .IsRequired();

            builder.HasOne(x => x.GcReportStudentGrade)
             .WithMany(x => x.GcReportStudents)
             .HasForeignKey(fk => fk.IdGcReportStudentGrade)
             .HasConstraintName("FK_TrGcReportStudent_TrGcReportStudentGrade")
             .OnDelete(DeleteBehavior.Restrict);

            base.Configure(builder);
        }
    }
}
