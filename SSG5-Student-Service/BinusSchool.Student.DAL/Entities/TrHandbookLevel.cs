using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities.School;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace BinusSchool.Persistence.StudentDb.Entities
{
    public class TrHandbookLevel : AuditEntity, IStudentEntity
    {
        public string IdTrHandbook { get; set; }
        public string IdLevel { get; set; }
        public virtual TrHandbook Handbook { get; set; }
        public virtual MsLevel Level { get; set; }
    }

    internal class TrHandbookLevelConfiguration : AuditEntityConfiguration<TrHandbookLevel>
    {
        public override void Configure(EntityTypeBuilder<TrHandbookLevel> builder)
        {
            builder.HasOne(x => x.Handbook)
              .WithMany(x => x.HandbookViewLevel)
              .HasForeignKey(fk => fk.IdTrHandbook)
              .HasConstraintName("FK_TrHandbookLevel_TrHandbook")
              .OnDelete(DeleteBehavior.Restrict)
              .IsRequired();

            builder.HasOne(x => x.Level)
             .WithMany(x => x.HandbookViewLevel)
             .HasForeignKey(fk => fk.IdLevel)
             .HasConstraintName("FK_TrHandbookLevel_MsLevel")
             .IsRequired();

            base.Configure(builder);
        }
    }
}
