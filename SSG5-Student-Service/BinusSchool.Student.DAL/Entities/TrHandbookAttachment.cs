using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities.School;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace BinusSchool.Persistence.StudentDb.Entities
{
    public class TrHandbookAttachment : AuditEntity, IStudentEntity
    {
        public string IdTrHandbook { get; set; }
        public string OriginalFilename { get; set; }
        public string Url { get; set; }
        public string Filename { get; set; }
        public string Filetype { get; set; }
        public decimal Filesize { get; set; }
        public virtual TrHandbook Handbook { get; set; }
        public virtual MsLevel Level { get; set; }
    }

    internal class TrHandbookAttachmentConfiguration : AuditEntityConfiguration<TrHandbookAttachment>
    {
        public override void Configure(EntityTypeBuilder<TrHandbookAttachment> builder)
        {
            builder.HasOne(x => x.Handbook)
              .WithMany(x => x.HandbookAttachment)
              .HasForeignKey(fk => fk.IdTrHandbook)
              .HasConstraintName("FK_TrHandbookAttachment_TrHandbook")
              .OnDelete(DeleteBehavior.Restrict)
              .IsRequired();

            base.Configure(builder);
        }
    }
}
