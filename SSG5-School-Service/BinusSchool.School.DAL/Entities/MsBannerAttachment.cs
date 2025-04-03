using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchoolDb.Entities
{
    public class MsBannerAttachment : CodeEntity, ISchoolEntity
    {
        public string IdBanner { get; set; }
        public string OriginalFilename { get; set; }
        public string Url { get; set; }
        public string Filename { get; set; }
        public string Filetype { get; set; }
        public decimal Filesize { get; set; }
        public bool IsImage { get; set; }
        public virtual MsBanner Banner { get; set; }
    }

    internal class MsBannerAttachmentConfiguration : CodeEntityConfiguration<MsBannerAttachment>
    {
        public override void Configure(EntityTypeBuilder<MsBannerAttachment> builder)
        {
            builder.Property(x => x.IsImage)
                .IsRequired();

            builder.Property(p => p.Url).HasMaxLength(450).IsRequired();
            builder.Property(p => p.Filename).HasMaxLength(100).IsRequired();
            builder.Property(p => p.OriginalFilename).HasMaxLength(100).IsRequired();
            builder.Property(p => p.Filetype).HasMaxLength(10).IsRequired();
            builder.Property(x => x.Filesize).IsRequired(true);

            builder.HasOne(x => x.Banner)
               .WithMany(x => x.BannerAttachments)
               .HasForeignKey(fk => fk.IdBanner)
               .HasConstraintName("FK_MsBannerAttachment_MsBanner")
               .OnDelete(DeleteBehavior.Cascade)
               .IsRequired();


            base.Configure(builder);
        }
    }
}
