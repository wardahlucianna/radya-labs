using System;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities
{
    public class TrStudentSafeReport : AuditEntity, IStudentEntity
    {
        public string IdStudent { get; set; }
        public DateTime Date { get; set; }

        public virtual MsStudent MsStudent { get; set; }
    }

    internal class TrStudentSafeReportConfiguration : AuditEntityConfiguration<TrStudentSafeReport>
    {
        public override void Configure(EntityTypeBuilder<TrStudentSafeReport> builder)
        {
            builder.Property(x => x.IdStudent)
                .HasMaxLength(36)
                .IsRequired();

            builder.HasOne(x => x.MsStudent)
                .WithMany(x => x.TrStudentSafeReports)
                .IsRequired()
                .HasForeignKey(fk => fk.IdStudent)
                .HasConstraintName("FK_TrStudentSafeReport_MsStudent")
                .OnDelete(DeleteBehavior.Cascade);

            base.Configure(builder);
        }
    }
}
