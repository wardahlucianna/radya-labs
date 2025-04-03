using System;
using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities.School;
using BinusSchool.Persistence.StudentDb.Entities.Scheduling;
using BinusSchool.Persistence.StudentDb.Entities.User;
using BinusSchool.Common.Model.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities
{
    public class TrEvidencesAttachment : AuditEntity, IStudentEntity
    {
        public string IdEvidences { get; set; }
        public string Url { get; set; }
        public string File { get; set; }
        public int Size { get; set; }
        public string FileType { get; set; }
        public virtual TrEvidences Evidences { get; set; }
    }

    internal class TrEvidencesAttachmentConfiguration : AuditEntityConfiguration<TrEvidencesAttachment>
    {
        public override void Configure(EntityTypeBuilder<TrEvidencesAttachment> builder)
        {
            builder.Property(x => x.IdEvidences)
                .HasMaxLength(36)
                .IsRequired();

            builder.Property(x => x.Url)
                .HasMaxLength(256)
                .IsRequired();

            builder.Property(x => x.File)
                .IsRequired();

            builder.Property(x => x.FileType)
                .HasMaxLength(10)
                .IsRequired();

            builder.HasOne(x => x.Evidences)
                .WithMany(x => x.TrEvidencesAttachments)
                .IsRequired()
                .HasForeignKey(fk => fk.IdEvidences)
                .HasConstraintName("FK_TrEvidencesAttachment_TrEvidences")
                .OnDelete(DeleteBehavior.Cascade);

            base.Configure(builder);
        }
    }
}
