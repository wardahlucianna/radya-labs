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
    public class MsExtracurricularScoreCompCategory : AuditEntity, ISchedulingEntity
    {
        public string IdAcademicYear { get; set; }
        public string Description { get; set; }
        public string IdExtracurricularScoreCalculationType { get; set; }
        public virtual MsAcademicYear AcademicYear { get; set; }
        public virtual MsExtracurricularScoreCalculationType ExtracurricularScoreCalculationType { get; set; }
        public virtual ICollection<MsExtracurricularScoreComponent> ExtracurricularScoreComponents { get; set; }
        public virtual ICollection<MsExtracurricularScoreCompMapping> ExtracurricularScoreCompMappings { get; set; }
    }
    internal class MsExtracurricularScoreCompCategoryConfiguration : AuditEntityConfiguration<MsExtracurricularScoreCompCategory>
    {
        public override void Configure(EntityTypeBuilder<MsExtracurricularScoreCompCategory> builder)
        {
            builder.Property(x => x.IdAcademicYear)
                .HasMaxLength(36)
                .IsRequired();

            builder.Property(x => x.IdExtracurricularScoreCalculationType)
                .HasMaxLength(36);
                //.IsRequired();

            builder.Property(x => x.Description)
                .HasMaxLength(128)
                .IsRequired();

            builder.HasOne(x => x.AcademicYear)
                .WithMany(x => x.ExtracurricularScoreCompCategorys)
                .HasForeignKey(fk => fk.IdAcademicYear)
                .HasConstraintName("FK_MsExtracurricularScoreCompCategory_MsAcademicYear")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            builder.HasOne(x => x.ExtracurricularScoreCalculationType)
                .WithMany(x => x.ExtracurricularScoreCompCategories)
                .HasForeignKey(fk => fk.IdExtracurricularScoreCalculationType)
                .HasConstraintName("FK_MsExtracurricularScoreCompCategory_MsExtracurricularScoreCalculationType")
                .OnDelete(DeleteBehavior.NoAction);
                //.IsRequired();

            base.Configure(builder);
        }
    }
}
