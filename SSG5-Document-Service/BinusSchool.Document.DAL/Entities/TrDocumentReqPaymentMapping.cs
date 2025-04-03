using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.DocumentDb.Abstractions;
using BinusSchool.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.DocumentDb.Entities
{
    public class TrDocumentReqPaymentMapping : AuditEntity, IDocumentEntity
    {
        public decimal TotalAmountReal { get; set; }
        public decimal TotalAmountInvoice { get; set; }
        public string IdDocumentReqApplicant { get; set; }
        public string IdDocumentReqPaymentMethod { get; set; }
        public string IdDocumentReqPaymentManual { get; set; }
        public bool UsingManualVerification { get; set; }
        public bool IsVirtualAccount { get; set; }
        public DateTime? StartDatePayment { get; set; }
        public DateTime? EndDatePayment { get; set; }
        public virtual MsDocumentReqApplicant DocumentReqApplicant { get; set; }
        public virtual LtDocumentReqPaymentMethod LtDocumentReqPaymentMethod { get; set; }
        public virtual TrDocumentReqPaymentManual DocumentReqPaymentManual { get; set; }
    }

    internal class TrDocumentReqPaymentMappingConfiguration : AuditEntityConfiguration<TrDocumentReqPaymentMapping>
    {
        public override void Configure(EntityTypeBuilder<TrDocumentReqPaymentMapping> builder)
        {
            builder.Property(x => x.TotalAmountReal)
               .HasColumnType("money")
               .IsRequired();

            builder.Property(x => x.TotalAmountInvoice)
               .HasColumnType("money")
               .IsRequired();

            builder.Property(x => x.IdDocumentReqApplicant)
                .HasMaxLength(36)
                .IsRequired();

            builder.Property(x => x.IdDocumentReqPaymentMethod)
                .HasMaxLength(36)
                .IsRequired();

            builder.Property(x => x.IdDocumentReqPaymentManual)
                .HasMaxLength(36);

            builder.HasOne(x => x.DocumentReqApplicant)
                .WithMany(x => x.DocumentReqPaymentMappings)
                .HasForeignKey(fk => fk.IdDocumentReqApplicant)
                .HasConstraintName("FK_TrDocumentReqPaymentMapping_MsDocumentReqApplicant")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            builder.HasOne(x => x.LtDocumentReqPaymentMethod)
                .WithMany(x => x.DocumentReqPaymentMappings)
                .HasForeignKey(fk => fk.IdDocumentReqPaymentMethod)
                .HasConstraintName("FK_TrDocumentReqPaymentMapping_LtDocumentReqPaymentMethod")
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            builder.HasOne(x => x.DocumentReqPaymentManual)
                .WithMany(x => x.DocumentReqPaymentMappings)
                .HasForeignKey(fk => fk.IdDocumentReqPaymentManual)
                .HasConstraintName("FK_TrDocumentReqPaymentMapping_TrDocumentReqPaymentManual")
                .OnDelete(DeleteBehavior.NoAction);

            base.Configure(builder);
        }
    }
}
