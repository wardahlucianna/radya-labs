using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace BinusSchool.Persistence.StudentDb.Entities
{
    public class TrCounselingServicesEntryAttachment : AuditEntity, IStudentEntity
    {

        public string IdCounselingServicesEntry { get; set; }
        public string OriginalName { get; set; }
        public int FileSize { get; set; }
        public string FileName { get; set; }
        public string Url { get; set; }
        public string FileType { get; set; }

        public virtual TrCounselingServicesEntry CounselingServicesEntry { get; set; }
    }

    internal class TrCounselingServicesEntryAttachmentConfiguration : AuditEntityConfiguration<TrCounselingServicesEntryAttachment>
    {
        public override void Configure(EntityTypeBuilder<TrCounselingServicesEntryAttachment> builder)
        {
            builder.Property(p => p.Url).HasMaxLength(450);
            builder.Property(p => p.OriginalName).HasMaxLength(100);
            builder.Property(p => p.FileName).HasMaxLength(100);
            builder.Property(p => p.FileType).HasMaxLength(10);
            builder.Property(x => x.FileSize).HasColumnType("decimal(18,2)");

            builder.HasOne(x => x.CounselingServicesEntry)
             .WithMany(x => x.CounselingServicesEntryAttachment)
             .HasForeignKey(fk => fk.IdCounselingServicesEntry)
             .HasConstraintName("FK_TrCounselingServicesEntryAttachment_TrCounselingServicesEntry")
             .OnDelete(DeleteBehavior.Restrict)
             .IsRequired();

            base.Configure(builder);
        }
    }
}
