using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities.Student;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchedulingDb.Entities
{
    public class TrExtracurricularScoreEntry : AuditEntity, ISchedulingEntity
    {
        public string IdExtracurricular { get; set; }
        public string IdExtracurricularScoreLegend { get; set; }
        public string IdStudent { get; set; }
        public string IdExtracurricularScoreComponent { get; set; }

        public virtual MsStudent Student { get; set; }
        public virtual MsExtracurricular Extracurricular { get; set; }
        public virtual MsExtracurricularScoreLegend ExtracurricularScoreLegend { get; set; }
        public virtual MsExtracurricularScoreComponent ExtracurricularScoreComponent { get; set; }
    }

    internal class TrExtracurricularScoreEntryConfiguration : AuditEntityConfiguration<TrExtracurricularScoreEntry>
    {
        public override void Configure(EntityTypeBuilder<TrExtracurricularScoreEntry> builder)
        {
            builder.Property(p => p.IdStudent)
                .HasMaxLength(36)
                .IsRequired();

            builder.Property(p => p.IdExtracurricular)
                .HasMaxLength(36)
                .IsRequired();

            builder.Property(p => p.IdExtracurricularScoreLegend)
                .HasMaxLength(36)
                .IsRequired();

            builder.Property(p => p.IdExtracurricularScoreComponent)
                .HasMaxLength(36);
                //.IsRequired();

            builder.HasOne(x => x.Student)
                .WithMany(x => x.ExtracurricularScoreEntries)
                .IsRequired()
                .HasForeignKey(fk => fk.IdStudent)
                .HasConstraintName("FK_TrExtracurricularScoreEntry_MsStudent")
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Extracurricular)
                .WithMany(x => x.ExtracurricularScoreEntries)
                .IsRequired()
                .HasForeignKey(fk => fk.IdExtracurricular)
                .HasConstraintName("FK_TrExtracurricularScoreEntry_MsExtracurricular")
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.ExtracurricularScoreLegend)
                .WithMany(x => x.ExtracurricularScoreEntries)
                .IsRequired()
                .HasForeignKey(fk => fk.IdExtracurricularScoreLegend)
                .HasConstraintName("FK_TrExtracurricularScoreEntry_MsExtracurricularScoreLegend")
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.ExtracurricularScoreComponent)
                .WithMany(x => x.ExtracurricularScoreEntries)
                .HasForeignKey(fk => fk.IdExtracurricularScoreComponent)
                .HasConstraintName("FK_TrExtracurricularScoreEntry_MsExtracurricularScoreComponent")
                .OnDelete(DeleteBehavior.NoAction);

            base.Configure(builder);
        }
    }
}
