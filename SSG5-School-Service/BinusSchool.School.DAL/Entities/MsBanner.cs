using System;
using System.Collections.Generic;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchoolDb.Entities
{
    public class MsBanner : CodeEntity, ISchoolEntity
    {
        public string Name { get; set; }
        public BannerOption Option { get; set; }
        public bool IsPin { get; set; }
        public string Link { get; set; }
        public DateTime PublishStartDate { get; set; }
        public DateTime PublishEndDate { get; set; }
        public string UrlImage { get; set; }
        public string Content { get; set; }
        public string IdSchool { get; set; }
        public virtual ICollection<MsBannerAttachment> BannerAttachments { get; set; }
        public virtual ICollection<MsBannerLevelGrade> BannerLevelGrades { get; set; }
        public virtual ICollection<MsBannerRole> BannerRoles { get; set; }
        public virtual MsSchool School { get; set; }
    }

    internal class MsBannerConfiguration : CodeEntityConfiguration<MsBanner>
    {
        public override void Configure(EntityTypeBuilder<MsBanner> builder)
        {
            builder.Property(e => e.Option).HasMaxLength(maxLength: 15)
               .HasConversion(valueToDb =>
               valueToDb.ToString(),
               valueFromDb => (BannerOption)Enum.Parse(typeof(BannerOption), valueFromDb))
               .IsRequired();

            builder.Property(x => x.Name)
             .HasMaxLength(1054)
             .IsRequired();

            builder.Property(x => x.Description)
             .HasMaxLength(1054)
             .IsRequired();

            builder.Property(x => x.Link)
                .HasMaxLength(1054)
                .IsRequired();

            builder.Property(x => x.PublishStartDate)
                .HasMaxLength(7)
                .IsRequired();

            builder.Property(x => x.PublishEndDate)
                .HasMaxLength(7)
                .IsRequired();

            builder.Property(p => p.UrlImage)
                .HasMaxLength(450)
                .IsRequired();

            builder.Property(p => p.Content)
                .IsRequired();

            builder.HasOne(x => x.School)
               .WithMany(x => x.Banners)
               .HasForeignKey(fk => fk.IdSchool)
               .HasConstraintName("FK_MsBanner_MsSchool")
               .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
