using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchedulingDb.Entities
{
    public class MsExtracurricularSupportDocGrade : AuditEntity, ISchedulingEntity
    {
        public string IdExtracurricularSupportDoc { get; set; }
        public string IdGrade { get; set; }

        public virtual MsGrade Grade { get; set; }
        public virtual MsExtracurricularSupportDoc ExtracurricularSupportDoc { get; set; }

    }

    internal class MsExtracurricularSupportDocGradeConfiguration : AuditEntityConfiguration<MsExtracurricularSupportDocGrade>
    {
        public override void Configure(EntityTypeBuilder<MsExtracurricularSupportDocGrade> builder)
        {
            builder.Property(x => x.IdExtracurricularSupportDoc)
             .HasMaxLength(36)
             .IsRequired();

            builder.Property(x => x.IdGrade)
             .HasMaxLength(36)
             .IsRequired();

            builder.HasOne(x => x.Grade)
                 .WithMany(y => y.ExtracurricularSupportDocGrades)
                 .HasForeignKey(fk => fk.IdGrade)
                 .HasConstraintName("FK_MsExtracurricularSupportDocGrade_MsGrade")
                 .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.ExtracurricularSupportDoc)
                 .WithMany(y => y.ExtracurricularSupportDocGrades)
                 .HasForeignKey(fk => fk.IdExtracurricularSupportDoc)
                 .HasConstraintName("FK_MsExtracurricularSupportDocGrade_MsExtracurricularSupportDoc")
                 .OnDelete(DeleteBehavior.Cascade);

            base.Configure(builder);
        }
    }
}
