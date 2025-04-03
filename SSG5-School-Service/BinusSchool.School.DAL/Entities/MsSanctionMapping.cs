using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchoolDb.Entities
{
    public class MsSanctionMapping : AuditEntity, ISchoolEntity
    {
        public string IdAcademicYear { get; set; }
        public string SanctionName { get; set; }
        public int Min { get; set; }
        public int Max { get; set; }
        public virtual MsAcademicYear AcademicYear { get; set; }
        public virtual ICollection<MsSanctionMappingAttentionBy> SanctionMappingAttentionBies { get; set; }
    }

    internal class MsSanctionMappingConfiguration : AuditEntityConfiguration<MsSanctionMapping>
    {
        public override void Configure(EntityTypeBuilder<MsSanctionMapping> builder)
        {
            builder.Property(x => x.SanctionName).HasMaxLength(100).IsRequired();

            builder.HasOne(x => x.AcademicYear)
                .WithMany(x => x.SanctionMappings)
                .HasForeignKey(fk => fk.IdAcademicYear)
                .HasConstraintName("FK_MsSanctionMapping_MsAcademicYear")
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
