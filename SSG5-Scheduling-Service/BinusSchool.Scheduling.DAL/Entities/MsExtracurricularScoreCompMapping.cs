using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchedulingDb.Entities
{
    public class MsExtracurricularScoreCompMapping : AuditEntity, ISchedulingEntity
    {
        public string IdExtracurricular { get; set; }
        public string IdExtracurricularScoreCompCategory { get; set; }

        public virtual MsExtracurricular Extracurricular { get; set; }
        public virtual MsExtracurricularScoreCompCategory ExtracurricularScoreCompCategory { get; set; }
    }

    internal class MsExtracurricularScoreCompMappingConfiguration : AuditEntityConfiguration<MsExtracurricularScoreCompMapping>
    {
        public override void Configure(EntityTypeBuilder<MsExtracurricularScoreCompMapping> builder)
        {

            builder.Property(x => x.IdExtracurricular)
              .HasMaxLength(36)
              .IsRequired();

            builder.Property(x => x.IdExtracurricularScoreCompCategory)
              .HasMaxLength(36)
              .IsRequired();


            builder.HasOne(x => x.Extracurricular)
                .WithMany(x => x.ExtracurricularScoreCompMappings)
                .IsRequired()
                .HasForeignKey(fk => fk.IdExtracurricular)
                .HasConstraintName("FK_MsExtracurricularScoreCompMapping_MsExtracurricular")
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.ExtracurricularScoreCompCategory)
              .WithMany(x => x.ExtracurricularScoreCompMappings)
              .IsRequired()
              .HasForeignKey(fk => fk.IdExtracurricularScoreCompCategory)
              .HasConstraintName("FK_MsExtracurricularScoreCompMapping_MsExtracurricularScoreCompCategory")
              .OnDelete(DeleteBehavior.Restrict);



            base.Configure(builder);
        }
    }
}
