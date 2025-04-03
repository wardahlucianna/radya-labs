using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.DocumentDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Collections.Generic;
using BinusSchool.Persistence.DocumentDb.Entities.School;

namespace BinusSchool.Persistence.DocumentDb.Entities
{
    public class LtBLPGroup : AuditEntity, IDocumentEntity
    {
        public string IdLevel { get; set; }
        public string GroupName { get; set; }

        public virtual MsLevel Level { get; set; }
        public virtual ICollection<TrBLPGroupStudent> BLPGroupStudents { get; set; }
        public virtual ICollection<MsClearanceWeekPeriod> BLPWeekPeriods { get; set; }
    }

    internal class LtBLPGroupConfiguration : AuditEntityConfiguration<LtBLPGroup>
    {
        public override void Configure(EntityTypeBuilder<LtBLPGroup> builder)
        {
            builder.Property(x => x.IdLevel)
                .HasMaxLength(36)
                .IsRequired();

            builder.Property(x => x.GroupName)
               .HasMaxLength(100)
               .IsRequired();

            builder.HasOne(x => x.Level)
               .WithMany(x => x.BLPGroups)
               .HasForeignKey(fk => fk.IdLevel)
               .HasConstraintName("FK_LtBLPGroup_MsLevel")
               .OnDelete(DeleteBehavior.Cascade)
               .IsRequired();

            base.Configure(builder);
        }
    }
}
