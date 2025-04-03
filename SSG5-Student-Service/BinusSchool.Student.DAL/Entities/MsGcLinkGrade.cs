using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities.School;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities
{
    public class MsGcLinkGrade : AuditEntity, IStudentEntity
    {
         public string IdGcLink { get; set; }
         public string IdGrade { get; set; }
         public virtual MsGcLink GcLink { get; set; }
         public virtual MsGrade Grade { get; set; }
    }

    internal class MsGcLinkGradeConfiguration : AuditEntityConfiguration<MsGcLinkGrade>
    {
        public override void Configure(EntityTypeBuilder<MsGcLinkGrade> builder)
        {
            builder.HasOne(x => x.GcLink)
             .WithMany(x => x.GcLinkGrades)
             .HasForeignKey(fk => fk.IdGcLink)
             .HasConstraintName("FK_MsGcLinkGrade_MsGcLink")
             .OnDelete(DeleteBehavior.Restrict)
             .IsRequired();

            builder.HasOne(x => x.Grade)
             .WithMany(x => x.GcLinkGrades)
             .HasForeignKey(fk => fk.IdGrade)
             .HasConstraintName("FK_MsGcLinkGrade_MsGrade")
             .OnDelete(DeleteBehavior.Restrict)
             .IsRequired();

            base.Configure(builder);
        }
    }
}
