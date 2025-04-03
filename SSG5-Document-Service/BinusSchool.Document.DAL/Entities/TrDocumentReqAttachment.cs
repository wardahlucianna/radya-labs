using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.DocumentDb.Abstractions;
using BinusSchool.Persistence.DocumentDb.Entities.User;
using BinusSchool.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.DocumentDb.Entities
{
    public class TrDocumentReqAttachment : AuditEntity, IDocumentEntity
    {
        public string IdDocumentReqApplicantDetail { get; set; }
        public string OriginalFileName { get; set; }
        public string FileName { get; set; }
        public string FileType { get; set; }
        public string FileExtension { get; set; }
        public decimal FileSize { get; set; }
        public string FilePath { get; set; }
        public bool ShowToParent { get; set; }
        public string IdUserModifier { get; set; }
        public virtual TrDocumentReqApplicantDetail DocumentReqApplicantDetail { get; set; }
        public virtual MsUser User { get; set; }
    }

    internal class TrDocumentReqAttachmentConfiguration : AuditEntityConfiguration<TrDocumentReqAttachment>
    {
        public override void Configure(EntityTypeBuilder<TrDocumentReqAttachment> builder)
        {
            builder.Property(x => x.IdDocumentReqApplicantDetail)
                .HasMaxLength(36)
                .IsRequired();

            builder.Property(x => x.OriginalFileName)
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(x => x.FileName)
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(x => x.FileType)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(x => x.FileExtension)
                .HasMaxLength(10)
                .IsRequired();

            builder.Property(x => x.FileSize)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(x => x.FilePath)
                .HasMaxLength(500)
                .IsRequired();

            builder.Property(x => x.IdUserModifier)
                .HasMaxLength(36)
                .IsRequired();

            builder.HasOne(x => x.DocumentReqApplicantDetail)
                .WithMany(x => x.DocumentReqAttachments)
                .HasForeignKey(fk => fk.IdDocumentReqApplicantDetail)
                .HasConstraintName("FK_TrDocumentReqAttachment_TrDocumentReqApplicantDetail")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder.HasOne(x => x.User)
                .WithMany(x => x.DocumentReqAttachments)
                .HasForeignKey(fk => fk.IdUserModifier)
                .HasConstraintName("FK_TrDocumentReqAttachment_MsUser")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
