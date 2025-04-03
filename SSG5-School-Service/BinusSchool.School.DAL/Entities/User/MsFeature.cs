using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchoolDb.Entities.User
{
    public class MsFeature : CodeEntity, ISchoolEntity
    {
        public string IdParent { get; set; }
        public int OrderNumber { get; set; }
        public bool IsShowMobile { get; set; }
        public virtual MsFeature Parent { get; set; }
        public virtual ICollection<MsFeatureSchool> FeatureSchools { get; set; }
        public virtual ICollection<MsFeature> Childs { get; set; }
        public virtual ICollection<MsProjectFeedback> ProjectFeedbacks { get; set; }
        public virtual ICollection<MsProjectFeedback> SubProjectFeedbacks { get; set; }
    }

    internal class MsFeatureConfiguration : CodeEntityConfiguration<MsFeature>
    {
        public override void Configure(EntityTypeBuilder<MsFeature> builder)
        {
            builder.Property(x => x.OrderNumber)
                .IsRequired();

            builder.HasOne(x => x.Parent)
               .WithMany(x => x.Childs)
               .HasForeignKey(fk => fk.IdParent)
               .HasConstraintName("FK_MsFeature_MsFeatureParent")
               .OnDelete(DeleteBehavior.NoAction);

            base.Configure(builder);
        }
    }
}
