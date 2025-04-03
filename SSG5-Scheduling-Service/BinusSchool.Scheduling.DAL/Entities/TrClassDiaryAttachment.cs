using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchedulingDb.Entities
{
    public class TrClassDiaryAttachment : AuditEntity, ISchedulingEntity
    {
        public string IdClassDiary { get; set; }
        public string OriginalFilename { get; set; }
        public string Url { get; set; }
        public string Filename { get; set; }
        public string Filetype { get; set; }
        /// <summary>
        /// File size in kb
        /// </summary>
        public decimal Filesize { get; set; }
        public virtual TrClassDiary ClassDiary { get; set; }
    }

    internal class TrClassDiaryAttachmentConfiguration : AuditEntityConfiguration<TrClassDiaryAttachment>
    {
        public override void Configure(EntityTypeBuilder<TrClassDiaryAttachment> builder)
        {
            builder.HasOne(x => x.ClassDiary)
               .WithMany(x => x.ClassDiaryAttachments)
               .HasForeignKey(fk => fk.IdClassDiary)
               .HasConstraintName("FK_TrClassDiaryAttachment_TrClassDiary")
               .OnDelete(DeleteBehavior.Restrict)
               .IsRequired();
            builder.Property(p => p.Url).HasMaxLength(450);
            builder.Property(p => p.OriginalFilename).HasMaxLength(100);
            builder.Property(p => p.Filename).HasMaxLength(200);
            builder.Property(p => p.Filetype).HasMaxLength(10);
            builder.Property(x => x.Filesize)
            .HasColumnType("decimal(18,2)");

            base.Configure(builder);
        }
    }
}
