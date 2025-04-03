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
    public class MsExtracurricularScoreLegend : AuditEntity, ISchedulingEntity
    {
        public string IdSchool { get; set; }
        public string Score { get; set; }
        public string Description { get; set; }
        //public bool IsDefault { get; set; }
        public string IdExtracurricularScoreLegendCategory { get; set; }
        
        public virtual MsSchool School { get; set; }
        public virtual MsExtracurricularScoreLegendCategory ExtracurricularScoreLegendCategory { get; set; }
        public virtual ICollection<TrExtracurricularScoreEntry> ExtracurricularScoreEntries { get; set; }
        //public virtual ICollection<MsExtracurricularScoreLegendMapping> ExtracurricularScoreLegendMappings { get; set; }

    }

    internal class MsExtracurricularScoreLegendConfiguration : AuditEntityConfiguration<MsExtracurricularScoreLegend>
    {
        public override void Configure(EntityTypeBuilder<MsExtracurricularScoreLegend> builder)
        {
            builder.Property(x => x.IdSchool)
              .HasMaxLength(36)
              .IsRequired();

            builder.Property(x => x.Score)
              .HasMaxLength(36)
              .IsRequired();

            builder.Property(x => x.Description)
                .HasMaxLength(128)
                .IsRequired();

            builder.HasOne(x => x.School)
                .WithMany(x => x.ExtracurricularScoreLegends)
                .HasForeignKey(fk => fk.IdSchool)
                .HasConstraintName("FK_MsExtracurricularScoreLegend_MsSchool")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();


            builder.HasOne(x => x.ExtracurricularScoreLegendCategory)
                .WithMany(x => x.ExtracurricularScoreLegends)
                .HasForeignKey(fk => fk.IdExtracurricularScoreLegendCategory)
                .HasConstraintName("FK_MsExtracurricularScoreLegend_MsExtracurricularScoreLegendCategory")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();


            builder.HasOne(x => x.ExtracurricularScoreLegendCategory)
                .WithMany(x => x.ExtracurricularScoreLegends)
                .HasForeignKey(fk => fk.IdExtracurricularScoreLegendCategory)
                .HasConstraintName("FK_MsExtracurricularScoreLegend_MsExtracurricularScoreLegendCategory")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
