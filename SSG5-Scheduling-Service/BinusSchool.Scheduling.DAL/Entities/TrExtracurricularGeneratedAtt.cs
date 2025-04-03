using System;
using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchedulingDb.Entities
{
    public class TrExtracurricularGeneratedAtt : AuditEntity, ISchedulingEntity
    {
        public string IdExtracurricular { get; set; }                       // FK: MsExtracurricular 
        public string IdExtracurricularSession { get; set; }               // FK: MsExtracurricularSession  
        public DateTime Date { get; set; }
        public bool NewSession { get; set; }
        public virtual MsExtracurricular Extracurricular { get; set; }
        public virtual MsExtracurricularSession ExtracurricularSession { get; set; }
        public virtual ICollection<TrExtracurricularAttendanceEntry> ExtracurricularAttendanceEntries { get; set; }
    }
    internal class TrExtracurricularGeneratedAttConfiguration : AuditEntityConfiguration<TrExtracurricularGeneratedAtt>
    {
        public override void Configure(EntityTypeBuilder<TrExtracurricularGeneratedAtt> builder)
        {
            builder.Property(x => x.IdExtracurricular)
               .HasMaxLength(36)
               .IsRequired();

            builder.Property(x => x.IdExtracurricularSession)
               .HasMaxLength(36);

            builder.HasOne(x => x.Extracurricular)
                .WithMany(x => x.ExtracurricularGeneratedAtts)
                .HasForeignKey(fk => fk.IdExtracurricular)
                .HasConstraintName("FK_TrExtracurricularGeneratedAtt_MsExtracurricular")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            builder.HasOne(x => x.ExtracurricularSession)
                .WithMany(x => x.ExtracurricularGeneratedAtts)
                .HasForeignKey(fk => fk.IdExtracurricularSession)
                .HasConstraintName("FK_TrExtracurricularGeneratedAtt_MsExtracurricularSession")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
