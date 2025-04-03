using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchedulingDb.Entities
{
    public class MsExtracurricularObjective : AuditEntity, ISchedulingEntity
    {
        public string IdExtracurricular { get; set; }
        public string Description { get; set; }
        public int OrderNumber { get; set; }

        public virtual MsExtracurricular Extracurricular { get; set; }
    }

    internal class MsExtracurricularObjectiveConfiguration : AuditEntityConfiguration<MsExtracurricularObjective>
    {
        public override void Configure(EntityTypeBuilder<MsExtracurricularObjective> builder)
        {
            builder.Property(x => x.IdExtracurricular)
                .HasMaxLength(36)
                .IsRequired();

            builder.Property(x => x.Description)
                .HasMaxLength(1000)
                .IsRequired();

            builder.HasOne(x => x.Extracurricular)
               .WithMany(x => x.ExtracurricularObjectives)
               .HasForeignKey(fk => fk.IdExtracurricular)
               .HasConstraintName("FK_MsExtracurricularObjective_MsExtracurricular")
               .OnDelete(DeleteBehavior.Cascade)
               .IsRequired();

            base.Configure(builder);
        }
    }
}


