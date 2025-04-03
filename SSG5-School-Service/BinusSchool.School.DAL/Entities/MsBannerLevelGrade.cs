using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchoolDb.Entities
{
    public class MsBannerLevelGrade : CodeEntity, ISchoolEntity
    {
        public string IdBanner { get; set; }
        public string IdGrade { get; set; }
        public virtual MsBanner Banner { get; set; }
        public virtual MsGrade Grade { get; set; }
    }

    internal class MsBannerLevelGradeConfiguration : CodeEntityConfiguration<MsBannerLevelGrade>
    {
        public override void Configure(EntityTypeBuilder<MsBannerLevelGrade> builder)
        {
            builder.HasOne(x => x.Banner)
               .WithMany(x => x.BannerLevelGrades)
               .HasForeignKey(fk => fk.IdBanner)
               .HasConstraintName("FK_MsBannerLevelGrade_MsBanner")
               .OnDelete(DeleteBehavior.NoAction)
               .IsRequired();

            builder.HasOne(x => x.Grade)
               .WithMany(x => x.BannerLevelGrades)
               .HasForeignKey(fk => fk.IdGrade)
               .HasConstraintName("FK_MsBennerLevelGrade_MsGrade")
               .OnDelete(DeleteBehavior.NoAction)
               .IsRequired();


            base.Configure(builder);
        }
    }
}
