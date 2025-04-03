using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities
{
    public class TrExemplaryAttachment : AuditEntity, IStudentEntity
    {
        public string IdExemplary { get; set; }
        public string OriginalFileName { get; set; }
        public string Url { get; set; }
        public string FileName { get; set; }
        public string FileType { get; set; }
        public decimal FileSize { get; set; }
        public string FileExtension { get; set; }
        public virtual TrExemplary Exemplary { get; set; }
    }

    internal class TrExemplaryAttachmentConfiguration : AuditEntityConfiguration<TrExemplaryAttachment>
    {
        public override void Configure(EntityTypeBuilder<TrExemplaryAttachment> builder)
        {
            builder.Property(x => x.IdExemplary)
               .HasMaxLength(36)
               .IsRequired();

            builder.Property(x => x.OriginalFileName)
               .HasMaxLength(500);

            builder.Property(x => x.Url)
               .HasMaxLength(500);

            builder.Property(x => x.FileName)
               .HasMaxLength(500);

            builder.Property(x => x.FileType)
               .HasMaxLength(500);

            builder.Property(x => x.FileExtension)
               .HasMaxLength(10);

            builder.Property(x => x.FileSize)
                .HasColumnType("decimal(18,2)");

            builder.HasOne(x => x.Exemplary)
                .WithMany(x => x.ExemplaryAttachments)
                .IsRequired()
                .HasForeignKey(fk => fk.IdExemplary)
                .HasConstraintName("FK_TrExemplaryAttachment_TrExemplary")
                .OnDelete(DeleteBehavior.Cascade);

            base.Configure(builder);
        }
    }
}
