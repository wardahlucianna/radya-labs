using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.UserDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.UserDb.Entities
{
    public class MsBlockingTypeSubFeature : AuditEntity, IUserEntity
    {
        public string IdBlockingType { get; set; }
        public string IdSubFeature { get; set; }
        public virtual MsBlockingType BlockingType { get; set; }
        public virtual MsFeature Feature { get; set; }
    }

    internal class MsBlockingTypeSubFeatureConfiguration : AuditEntityConfiguration<MsBlockingTypeSubFeature>
    {
        public override void Configure(EntityTypeBuilder<MsBlockingTypeSubFeature> builder)
        {
            builder.HasOne(x => x.BlockingType)
                .WithMany(x => x.BlockingTypeSubFeature)
                .HasForeignKey(fk => fk.IdBlockingType)
                .HasConstraintName("FK_MsBlockingTypeSubFeature_MsBlockingTypeSub")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder.HasOne(x => x.Feature)
                .WithMany(x => x.BlockingTypeSubFeatures)
                .HasForeignKey(fk => fk.IdSubFeature)
                .HasConstraintName("FK_MsBlockingTypeSubFeature_MsFeature")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
