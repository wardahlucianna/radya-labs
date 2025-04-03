using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.UserDb.Abstractions;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.UserDb.Entities
{
    public class LtPermission : CodeEntity, IUserEntity
    {
        public virtual ICollection<MsFeaturePermission> FeaturePermissions { get; set; }
    }

    internal class LtPermissionConfiguration : CodeEntityConfiguration<LtPermission>
    {
        public override void Configure(EntityTypeBuilder<LtPermission> builder)
        {
            base.Configure(builder);

            builder.Property(x => x.Code)
                .HasMaxLength(36);
        }
    }
}
