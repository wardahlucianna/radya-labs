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
    public class TrExtracurricularExternalCoachAtt : AuditEntity, ISchedulingEntity
    {
        public string IdAcademicYear { get; set; }
        public int Semester { get; set; }
        public string IdExtracurricularExternalCoach { get; set; }
        public string IdExtracurricular { get; set; }
        public DateTime AttendanceDateTime { get; set; }
        public virtual MsAcademicYear AcademicYear { get; set; }
        public virtual MsExtracurricularExternalCoach ExtracurricularExternalCoach { get; set; }
        public virtual MsExtracurricular Extracurricular { get; set; }
    }
    internal class TrExtracurricularExternalCoachAttConfiguration : AuditEntityConfiguration<TrExtracurricularExternalCoachAtt>
    {
        public override void Configure(EntityTypeBuilder<TrExtracurricularExternalCoachAtt> builder)
        {
            builder.Property(x => x.IdAcademicYear)
            .HasMaxLength(36)
            .IsRequired();                     

            builder.Property(x => x.IdExtracurricularExternalCoach)
            .HasMaxLength(36)
            .IsRequired();

            builder.Property(x => x.AttendanceDateTime)
            .HasColumnType("datetime2");

            builder.Property(x => x.IdExtracurricular)
            .HasMaxLength(36)
            .IsRequired();

            builder.HasOne(x => x.AcademicYear)
                .WithMany(y => y.ExtracurricularExternalCoachAtts)
                .HasForeignKey(fk => fk.IdAcademicYear)
                .HasConstraintName("FK_TrExtracurricularExternalCoachAtt_MsAcademicYear")
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.ExtracurricularExternalCoach)
                 .WithMany(y => y.ExtracurricularExternalCoachAtts)
                 .HasForeignKey(fk => fk.IdExtracurricularExternalCoach)
                 .HasConstraintName("FK_TrExtracurricularExternalCoachAtt_MsExtracurricularExternalCoach")
                 .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.Extracurricular)
                .WithMany(y => y.ExtracurricularExternalCoachAtts)
                .HasForeignKey(fk => fk.IdExtracurricular)
                .HasConstraintName("FK_TrExtracurricularExternalCoachAtt_MsExtracurricular")
                .OnDelete(DeleteBehavior.NoAction);


            base.Configure(builder);
        }
    }
}
