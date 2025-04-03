using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.UserDb.Abstractions;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.UserDb.Entities
{
    public class TrHierarchyMapping : AuditEntity, IUserEntity
    {
        public string Name { get; set; }
        public virtual ICollection<TrHierarchyMappingDetail> HierarchyMappingDetails { get; set; }
    }

    internal class TrHierarchyMappingConfiguration : AuditEntityConfiguration<TrHierarchyMapping>
    {
        public override void Configure(EntityTypeBuilder<TrHierarchyMapping> builder)
        {
            builder.Property(x => x.Name).HasMaxLength(50);
            base.Configure(builder);
        }
    }
}
