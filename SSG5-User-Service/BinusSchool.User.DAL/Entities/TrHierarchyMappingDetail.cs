using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.UserDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.UserDb.Entities
{
    public class TrHierarchyMappingDetail : AuditEntity, IUserEntity
    {
        public string IdRolePosition { get; set; }
        public string IdHierarchyMapping { get; set; }
        public string IdParent { get; set; }

        public virtual TrHierarchyMappingDetail Parent { get; set; }
        public virtual TrHierarchyMapping HierarchyMapping { get; set; }
        public virtual TrRolePosition RolePosition { get; set; }
        public virtual ICollection<TrHierarchyMappingDetail> Childs { get; set; }
    }

    internal class TrHierarchyMappingDetailConfiguration : AuditEntityConfiguration<TrHierarchyMappingDetail>
    {
        public override void Configure(EntityTypeBuilder<TrHierarchyMappingDetail> builder)
        {
            builder.HasOne(x => x.RolePosition)
              .WithMany(x => x.HierarchyMappingDetails)
              .HasForeignKey(fk => fk.IdRolePosition)
              .HasConstraintName("FK_TrHierarchyMappingDetail_TrRolePosition")
              .OnDelete(DeleteBehavior.NoAction)
              .IsRequired();

            builder.HasOne(x => x.HierarchyMapping)
              .WithMany(x => x.HierarchyMappingDetails)
              .HasForeignKey(fk => fk.IdHierarchyMapping)
              .HasConstraintName("FK_TrHierarchyMappingDetail_TrHierarchyMapping")
              .OnDelete(DeleteBehavior.NoAction)
              .IsRequired(); 

            builder.HasOne(x => x.Parent)
                .WithMany(x => x.Childs)
                .HasForeignKey(fk => fk.IdParent)
                .HasConstraintName("FK_TrHierarchyMappingDetail_TrHierarchyMappingDetail")
                .OnDelete(DeleteBehavior.NoAction);

            base.Configure(builder);
        }
    }
}
