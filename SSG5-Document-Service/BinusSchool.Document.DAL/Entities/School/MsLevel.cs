using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.DocumentDb.Abstractions;
using BinusSchool.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.DocumentDb.Entities.School
{
    public class MsLevel : CodeEntity, IDocumentEntity
    {
        public string IdAcademicYear { get; set; }
        public int OrderNumber { get; set; }

        public virtual MsAcademicYear AcademicYear { get; set; }
        public virtual ICollection<MsGrade> Grades { get; set; }
        public virtual ICollection<LtBLPGroup> BLPGroups { get; set; }
    }

    internal class MsLevelConfiguration : CodeEntityConfiguration<MsLevel>
    {
        public override void Configure(EntityTypeBuilder<MsLevel> builder)
        {
            builder.HasOne(x => x.AcademicYear)
                .WithMany(x => x.Levels)
                .HasForeignKey(fk => fk.IdAcademicYear)
                .HasConstraintName("FK_MsLevel_MsAcademicYear")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
