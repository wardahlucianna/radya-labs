using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.UserDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.UserDb.Entities
{
    public class MsFeature : CodeEntity, IUserEntity
    {
        public string IdParent { get; set; }
        public string Icon { get; set; }
        public string Controller { get; set; }
        public string Action { get; set; }
        public string ParamUrl { get; set; }
        public int OrderNumber { get; set; }
        public bool IsShowMobile { get; set; }
        public string Type { get; set; }

        public virtual MsFeature Parent { get; set; }
        public virtual ICollection<MsFeature> Childs { get; set; }
        public virtual ICollection<MsFeaturePermission> FeaturePermissions { get; set; }
        public virtual ICollection<MsFeatureSchool> FeatureSchools { get; set; }
        public virtual ICollection<MsBlockingType> BlockingTypes { get; set; }
        public virtual ICollection<MsBlockingTypeSubFeature> BlockingTypeSubFeatures { get; set; }

    }

    internal class MsFeatureConfiguration : CodeEntityConfiguration<MsFeature>
    {
        public override void Configure(EntityTypeBuilder<MsFeature> builder)
        {
            builder.Property(x => x.Icon)
                .HasMaxLength(32)
                .IsRequired();

            builder.Property(x => x.Controller)
                .HasMaxLength(64)
                .IsRequired();

            builder.Property(x => x.Action)
                .HasMaxLength(32);

            builder.Property(x => x.ParamUrl)
                .HasMaxLength(128);

            builder.Property(x => x.OrderNumber)
                .IsRequired();

            builder.HasOne(x => x.Parent)
                .WithMany(x => x.Childs)
                .HasForeignKey(fk => fk.IdParent)
                .HasConstraintName("FK_MsFeature_MsFeature")
                .OnDelete(DeleteBehavior.NoAction);

            base.Configure(builder);
        }
    }
}
