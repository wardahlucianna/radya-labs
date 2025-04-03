using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.UserDb.Abstractions;
using BinusSchool.Persistence.UserDb.Entities.School;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.UserDb.Entities
{
    public class MsFeatureSchool : AuditEntity, IUserEntity
    {
        public string IdSchool { get; set; }
        public string IdFeature { get; set; }

        public virtual MsSchool School { get; set; }
        public virtual MsFeature Feature { get; set; }
        public virtual ICollection<TrNotification> Notifications { get; set; }
    }

    internal class MsFeatureSchoolConfiguration : AuditEntityConfiguration<MsFeatureSchool>
    {
        public override void Configure(EntityTypeBuilder<MsFeatureSchool> builder)
        {
            builder.HasOne(x => x.School)
                .WithMany(x => x.FeatureSchools)
                .HasForeignKey(fk => fk.IdSchool)
                .HasConstraintName("FK_MsFeatureSchool_MsSchool")
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
            
            builder.HasOne(x => x.Feature)
                .WithMany(x => x.FeatureSchools)
                .HasForeignKey(fk => fk.IdFeature)
                .HasConstraintName("FK_MsFeatureSchool_MsFeature")
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            base.Configure(builder);
        }
    }
}
