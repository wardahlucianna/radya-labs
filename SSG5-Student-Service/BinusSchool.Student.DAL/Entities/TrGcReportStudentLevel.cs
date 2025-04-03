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
    public class TrGcReportStudentLevel : AuditEntity, IStudentEntity
    {
        public string IdLevel { get; set; }
        public virtual MsLevel Level { get; set; }
        public virtual ICollection<TrGcReportStudentGrade> GcReportStudentGrades { get; set; }
    }

    internal class TrGcReportStudentLevelConfiguration : AuditEntityConfiguration<TrGcReportStudentLevel>
    {
        public override void Configure(EntityTypeBuilder<TrGcReportStudentLevel> builder)
        {
            builder.HasOne(x => x.Level)
             .WithMany(x => x.GcReportStudentLevels)
             .HasForeignKey(fk => fk.IdLevel)
             .HasConstraintName("FK_TrGcReportStudentLevel_MsLevel")
             .OnDelete(DeleteBehavior.Restrict)
             .IsRequired();

            base.Configure(builder);
        }
    }
}
