using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities
{
    public class TrServiceAsActionUpload : AuditEntity, IStudentEntity
    {
        public string IdServiceAsActionEvidence { get; set; }
        public string EvidenceFIGM { get; set; }
        public string EvidenceText { get; set; }
        public string EvidenceURL { get; set; }

        public virtual TrServiceAsActionEvidence ServiceAsActionEvidence { get; set; }
    }

    internal class TrServiceAsActionUploadConfiguration : AuditEntityConfiguration<TrServiceAsActionUpload>
    {
        public override void Configure(EntityTypeBuilder<TrServiceAsActionUpload> builder)
        {
            builder.Property(x => x.IdServiceAsActionEvidence).IsRequired().HasMaxLength(36);
            builder.Property(x => x.EvidenceFIGM).HasMaxLength(200);
            builder.Property(x => x.EvidenceURL).HasMaxLength(2000);

            builder.HasOne(x => x.ServiceAsActionEvidence)
                .WithMany(x => x.Uploads)
                .HasForeignKey(fk => fk.IdServiceAsActionEvidence)
                .HasConstraintName("FK_TrServiceAsActionUpload_TrServiceAsActionEvidence")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
