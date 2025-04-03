using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchedulingDb.Entities
{
    public class HTrClassDiaryAttachment : AuditNoUniqueEntity, ISchedulingEntity
    {
        public string IdHTrClassDiaryAttachment { get; set; }
        public string IdHTrClassDiary { get; set; }
        public string OriginalFilename { get; set; }
        public string Url { get; set; }
        public string Filename { get; set; }
        public string Filetype { get; set; }
        /// <summary>
        /// File size in kb
        /// </summary>
        public decimal Filesize { get; set; }
        public virtual HTrClassDiary ClassDiary { get; set; }
    }

    internal class HTrClassDiaryAttachmentConfiguration : AuditNoUniqueEntityConfiguration<HTrClassDiaryAttachment>
    {
        public override void Configure(EntityTypeBuilder<HTrClassDiaryAttachment> builder)
        {
            builder.HasKey(x => x.IdHTrClassDiaryAttachment);
            builder.Property(p => p.IdHTrClassDiaryAttachment).HasMaxLength(36).IsRequired();
            builder.Property(p => p.IdHTrClassDiary).HasMaxLength(36);


            builder.HasOne(x => x.ClassDiary)
               .WithMany(x => x.HistoryClassDiaryAttachments)
               .HasForeignKey(fk => fk.IdHTrClassDiary)
               .HasConstraintName("FK_HTrClassDiaryAttachment_HTrClassDiary")
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
