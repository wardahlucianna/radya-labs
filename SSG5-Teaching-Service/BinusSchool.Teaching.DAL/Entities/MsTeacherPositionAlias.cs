using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.TeachingDb.Abstractions;
using BinusSchool.Persistence.TeachingDb.Entities.School;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.TeachingDb.Entities
{
    public class MsTeacherPositionAlias : AuditEntity, ITeachingEntity
    {
        public string IdTeacherPosition { get; set; }
        public string IdLevel { get; set; }
        public string Alias { get; set; }
        public virtual MsTeacherPosition TeacherPosition { get; set; }
        public virtual MsLevel Level { get; set; }

    }

    internal class MsTeacherPositionAliasConfiguration : AuditEntityConfiguration<MsTeacherPositionAlias>
    {
        public override void Configure(EntityTypeBuilder<MsTeacherPositionAlias> builder)
        {
            builder.HasOne(x => x.TeacherPosition)
                .WithMany(x => x.TeacherPositionAliases)
                .HasForeignKey(fk => fk.IdTeacherPosition)
                .HasConstraintName("FK_MsTeacherPositionAlias_MsTeacherPosition")
                .OnDelete(DeleteBehavior.Restrict)
                 .IsRequired();

            builder.HasOne(x => x.Level)
              .WithMany(x => x.TeacherPositionAliases)
              .HasForeignKey(fk => fk.IdLevel)
              .HasConstraintName("FK_MsTeacherPositionAlias_MsLevel")
              .OnDelete(DeleteBehavior.Restrict)
              .IsRequired();

            builder.Property(x=>x.Alias).HasMaxLength(256);

            base.Configure(builder);
        }
    }
}
