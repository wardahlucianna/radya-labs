using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchoolDb.Entities
{
    public class TrTextbookAttachment : AuditEntity, ISchoolEntity
    {
        public string IdTextbook { get; set; }
        public string Url { get; set; }
        public string FileName { get; set; }
        public string FileNameOriginal { get; set; }
        public string FileType { get; set; }
        public decimal FileSize { get; set; }
        public virtual TrTextbook Textbook { get; set; }
    }

    internal class TrTextbookAttachmentConfiguration : AuditEntityConfiguration<TrTextbookAttachment>
    {
        public override void Configure(EntityTypeBuilder<TrTextbookAttachment> builder)
        {
            builder.Property(x => x.Url)
               .HasMaxLength(450)
               .IsRequired();

            builder.Property(x => x.FileName)
               .HasMaxLength(200)
               .IsRequired();

            builder.Property(x => x.FileNameOriginal)
               .HasMaxLength(200)
               .IsRequired();

            builder.Property(x => x.FileType)
               .HasMaxLength(10)
               .IsRequired();

            builder.Property(x => x.FileSize)
               .IsRequired();

            builder.HasOne(x => x.Textbook)
                .WithMany(x => x.TextbookAttachments)
                .HasForeignKey(fk => fk.IdTextbook)
                .HasConstraintName("FK_TrTextbookAttacment_TrTextbook")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
