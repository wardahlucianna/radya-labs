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
    public class LtStudentDemoReportType : AuditEntity, IStudentEntity
    {
        public string Description { get; set; }
        public string Code { get; set; }
        public string IdSchool { get; set; }
        public virtual MsSchool School { get; set; }
    }

    internal class LtStudentDemoReportTypeConfiguration : AuditEntityConfiguration<LtStudentDemoReportType>
    {
        public override void Configure(EntityTypeBuilder<LtStudentDemoReportType> builder)
        {
            builder.Property(x => x.Description)
                .HasMaxLength(128)
                .IsRequired();

            builder.Property(x => x.Code)
                .HasMaxLength(36)
                .IsRequired();

            builder.Property(x => x.IdSchool)
                .HasMaxLength(36);

            builder.HasOne(x => x.School)
                .WithMany(x => x.StudentDemoReportTypes)
                .IsRequired()
                .HasForeignKey(fk => fk.IdSchool)
                .HasConstraintName("FK_LtStudentDemoReportType_MsSchool")
                .OnDelete(DeleteBehavior.Cascade);

            base.Configure(builder);
        }
    }
}
