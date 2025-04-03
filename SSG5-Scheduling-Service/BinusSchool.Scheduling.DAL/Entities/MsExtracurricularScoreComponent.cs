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
    public class MsExtracurricularScoreComponent : AuditEntity, ISchedulingEntity
    {
        public string IdAcademicYear { get; set; }
        public string Description { get; set; }
        public int OrderNumber { get; set; }

        public string IdExtracurricularScoreCompCategory { get; set; }
        

        public virtual MsAcademicYear AcademicYear { get; set; }
        public virtual MsExtracurricularScoreCompCategory ExtracurricularScoreCompCategory { get; set; }
        public virtual ICollection<TrExtracurricularScoreEntry> ExtracurricularScoreEntries { get; set; }
        //public virtual ICollection<MsExtracurricularScoreCompMapping> ExtracurricularScoreCompMappings { get; set; }
    }

    internal class MsExtracurricularScoreComponentConfiguration : AuditEntityConfiguration<MsExtracurricularScoreComponent>
    {
        public override void Configure(EntityTypeBuilder<MsExtracurricularScoreComponent> builder)
        {
            builder.Property(x => x.IdAcademicYear)
                .HasMaxLength(36)
                .IsRequired();

            builder.Property(x => x.Description)
                .HasMaxLength(128)
                .IsRequired();

            builder.Property(x => x.IdExtracurricularScoreCompCategory)
                .HasMaxLength(36)
                .IsRequired();

            builder.HasOne(x => x.AcademicYear)
                .WithMany(x => x.ExtracurricularScoreComponents)
                .IsRequired()
                .HasForeignKey(fk => fk.IdAcademicYear)
                .HasConstraintName("FK_MsExtracurricularScoreComponent_MsAcademicYear")
                .OnDelete(DeleteBehavior.NoAction);


            builder.HasOne(x => x.ExtracurricularScoreCompCategory)
                .WithMany(x => x.ExtracurricularScoreComponents)
                .IsRequired()
                .HasForeignKey(fk => fk.IdExtracurricularScoreCompCategory)
                .HasConstraintName("FK_MsExtracurricularScoreComponent_MsExtracurricularScoreCompCategory")
                .OnDelete(DeleteBehavior.Cascade);

            base.Configure(builder);
        }
    }
}


