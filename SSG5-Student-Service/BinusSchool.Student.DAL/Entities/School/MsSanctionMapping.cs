using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities.School
{
    public class MsSanctionMapping : AuditEntity, IStudentEntity
    {
        public string IdAcademicYear { get; set; }
        public string SanctionName { get; set; }
        public int Min { get; set; }
        public int Max { get; set; }
        public virtual MsAcademicYear AcademicYear { get; set; }
        public virtual ICollection<TrStudentPoint> StudentPoints { get; set; }
        public virtual ICollection<MsSanctionMappingAttentionBy> SanctionMappingAttentionBies { get; set; }
    }

    internal class MsSanctionMappingConfiguration : AuditEntityConfiguration<MsSanctionMapping>
    {
        public override void Configure(EntityTypeBuilder<MsSanctionMapping> builder)
        {
            builder.Property(x => x.SanctionName).HasMaxLength(50).IsRequired();

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
