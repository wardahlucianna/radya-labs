using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.DocumentDb.Abstractions;
using BinusSchool.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.DocumentDb.Entities
{
    public class TrDocumentReqPaymentManual : AuditEntity, IDocumentEntity
    {
        public decimal PaidAmount { get; set; }
        public int? PaymentStatus { get; set; }
        public DateTime? PaymentDate { get; set; }
        public string AttachmentFileName { get; set; }
        public string AttachmentImageUrl { get; set; }
        public string SenderAccountName { get; set; }
        public int? VerificationStatus { get; set; }
        public DateTime? VerificationDate { get; set; }
        public string Remarks { get; set; }
        public string IdBinusianVerificator { get; set; }
        public virtual MsStaff Staff { get; set; }
        public virtual ICollection<TrDocumentReqPaymentMapping> DocumentReqPaymentMappings { get; set; }
    }

    internal class TrDocumentReqPaymentManualConfiguration : AuditEntityConfiguration<TrDocumentReqPaymentManual>
    {
        public override void Configure(EntityTypeBuilder<TrDocumentReqPaymentManual> builder)
        {
            builder.Property(x => x.PaidAmount)
               .HasColumnType("money")
               .IsRequired();

            builder.Property(x => x.AttachmentFileName)
                .HasMaxLength(200);

            builder.Property(x => x.AttachmentImageUrl)
                .HasMaxLength(500);

            builder.Property(x => x.SenderAccountName)
                .HasMaxLength(250);

            builder.Property(x => x.Remarks)
                .HasMaxLength(500);

            builder.Property(x => x.IdBinusianVerificator)
                .HasMaxLength(36);

            builder.HasOne(x => x.Staff)
                .WithMany(x => x.DocumentReqPaymentManuals)
                .HasForeignKey(fk => fk.IdBinusianVerificator)
                .HasConstraintName("FK_TrDocumentReqPaymentManual_MsStaff")
                .OnDelete(DeleteBehavior.NoAction);

            base.Configure(builder);
        }
    }
}
