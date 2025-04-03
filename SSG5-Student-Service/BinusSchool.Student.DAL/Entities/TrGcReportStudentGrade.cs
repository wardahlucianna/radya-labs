using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities.School;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities
{
    public class TrGcReportStudentGrade : AuditEntity, IStudentEntity
    {
        public string IdGrade { get; set; }
        public string IdGcReportStudentLevel { get; set; }
        public virtual MsGrade Grade { get; set; }
        public virtual TrGcReportStudentLevel GcReportStudentLevel { get; set; }
        public virtual ICollection<TrGcReportStudent> GcReportStudents { get; set; }
    }

    internal class TrGcReportStudentGradeConfiguration : AuditEntityConfiguration<TrGcReportStudentGrade>
    {
        public override void Configure(EntityTypeBuilder<TrGcReportStudentGrade> builder)
        {
            builder.HasOne(x => x.Grade)
             .WithMany(x => x.GcReportStudentGrades)
             .HasForeignKey(fk => fk.IdGrade)
             .HasConstraintName("FK_TrGcReportStudentGrade_MsGrade")
             .OnDelete(DeleteBehavior.Restrict)
             .IsRequired();

            builder.HasOne(x => x.GcReportStudentLevel)
             .WithMany(x => x.GcReportStudentGrades)
             .HasForeignKey(fk => fk.IdGcReportStudentLevel)
             .HasConstraintName("FK_TrGcReportStudentGrade_TrGcReportStudentLevel")
             .OnDelete(DeleteBehavior.Restrict)
             .IsRequired();

            base.Configure(builder);
        }
    }
}
