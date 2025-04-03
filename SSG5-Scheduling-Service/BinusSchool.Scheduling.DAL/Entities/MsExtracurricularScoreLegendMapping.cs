using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchedulingDb.Entities
{
    public class MsExtracurricularScoreLegendMapping : AuditEntity, ISchedulingEntity
    {
        public string IdExtracurricular { get; set; }
        public string IdExtracurricularScoreLegendCategory { get; set; }
        

        public virtual MsExtracurricular Extracurricular { get; set; }
        public virtual MsExtracurricularScoreLegendCategory ExtracurricularScoreLegendCategory { get; set; }
        
    }
    internal class MsExtracurricularScoreLegendMappingConfiguration : AuditEntityConfiguration<MsExtracurricularScoreLegendMapping>
    {
        public override void Configure(EntityTypeBuilder<MsExtracurricularScoreLegendMapping> builder)
        {
            builder.Property(x => x.IdExtracurricular)
                .HasMaxLength(36)
                .IsRequired();

            builder.Property(x => x.IdExtracurricularScoreLegendCategory)
               .HasMaxLength(36)
               .IsRequired();


            builder.HasOne(x => x.Extracurricular)
                .WithMany(x => x.ExtracurricularScoreLegendMappings)
                .IsRequired()
                .HasForeignKey(fk => fk.IdExtracurricular)
                .HasConstraintName("FK_MsExtracurricularScoreLegendMapping_MsExtracurricular")
                .OnDelete(DeleteBehavior.Cascade);


            builder.HasOne(x => x.ExtracurricularScoreLegendCategory)
                .WithMany(x => x.ExtracurricularScoreLegendMappings)
                .IsRequired()
                .HasForeignKey(fk => fk.IdExtracurricularScoreLegendCategory)
                .HasConstraintName("FK_MsExtracurricularScoreLegendMapping_MsExtracurricularScoreLegendCategory")
                .OnDelete(DeleteBehavior.Restrict);

            base.Configure(builder);
        }
    }
}
