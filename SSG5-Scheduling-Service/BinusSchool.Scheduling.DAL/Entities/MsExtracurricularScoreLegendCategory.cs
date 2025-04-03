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
    public class MsExtracurricularScoreLegendCategory : AuditEntity, ISchedulingEntity
    {
        public string IdSchool { get; set; }
        public string Description { get; set; }
        public virtual MsSchool School { get; set; }
        public virtual ICollection<MsExtracurricularScoreLegendMapping> ExtracurricularScoreLegendMappings { get; set; }
        public virtual ICollection<MsExtracurricularScoreLegend> ExtracurricularScoreLegends { get; set; }


    }

    internal class MsExtracurricularScoreLegendCategoryConfiguration : AuditEntityConfiguration<MsExtracurricularScoreLegendCategory>
    {
        public override void Configure(EntityTypeBuilder<MsExtracurricularScoreLegendCategory> builder)
        {
            builder.Property(x => x.IdSchool)
              .HasMaxLength(36)
              .IsRequired();

            builder.Property(x => x.Description)
              .HasMaxLength(128)
              .IsRequired();

            builder.HasOne(x => x.School)
            .WithMany(x => x.ExtracurricularScoreLegendCategorys)
            .HasForeignKey(fk => fk.IdSchool)
            .HasConstraintName("FK_MsExtracurricularScoreLegendCategory_MsSchool")
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();

            base.Configure(builder);
        }
    }
}
