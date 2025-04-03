using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities.Employee;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchedulingDb.Entities
{
    public class MsExtracurricularSpvCoach : AuditEntity, ISchedulingEntity
    {
        public string IdExtracurricular { get; set; }
        public string IdBinusian { get; set; }
        public bool IsSpv { get; set; } //absolute
        public string IdExtracurricularCoachStatus { get; set; }              
        public virtual MsStaff Staff { get; set; }
        public virtual MsExtracurricular Extracurricular { get; set; }
        public virtual LtExtracurricularCoachStatus ExtracurricularCoachStatus { get; set; }
        
    }
    internal class MsExtracurricularSpvCoachConfiguration : AuditEntityConfiguration<MsExtracurricularSpvCoach>
    {
        public override void Configure(EntityTypeBuilder<MsExtracurricularSpvCoach> builder)
        {

            builder.Property(p => p.IdExtracurricular)
                .HasMaxLength(36)
                .IsRequired();

            builder.Property(p => p.IdBinusian)
                .HasMaxLength(36)
                .IsRequired();

            builder.Property(p => p.IdExtracurricularCoachStatus)
             .HasMaxLength(36);
             //.IsRequired();
            

            builder.HasOne(x => x.Staff)
                .WithMany(x => x.ExtracurricularSpvCoach)
                .IsRequired()
                .HasForeignKey(fk => fk.IdBinusian)
                .HasConstraintName("FK_MsExtracurricularSpvCoach_MsStaff")
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Extracurricular)
                .WithMany(x => x.ExtracurricularSpvCoach)
                .IsRequired()
                .HasForeignKey(fk => fk.IdExtracurricular)
                .HasConstraintName("FK_MsExtracurricularSpvCoach_MsExtracurricular")
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.ExtracurricularCoachStatus)
               .WithMany(x => x.ExtracurricularSpvCoachs)              
               .HasForeignKey(fk => fk.IdExtracurricularCoachStatus)
               .HasConstraintName("FK_MsExtracurricularSpvCoach_LtExtracurricularCoachStatus")
               .OnDelete(DeleteBehavior.Restrict);

            base.Configure(builder);

        }
    }

}
