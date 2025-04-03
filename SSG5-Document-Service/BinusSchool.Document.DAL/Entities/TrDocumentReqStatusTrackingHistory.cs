using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Persistence.DocumentDb.Abstractions;
using BinusSchool.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.DocumentDb.Entities
{
    public class TrDocumentReqStatusTrackingHistory : AuditEntity, IDocumentEntity
    {
        public string IdDocumentReqApplicant { get; set; }
        public DocumentRequestStatusWorkflow IdDocumentReqStatusWorkflow { get; set; }
        public string IdBinusianStaff { get; set; }
        public bool IsOnProcess { get; set; }
        public string Remarks { get; set; }
        public DateTime StatusDate { get; set; }
        public virtual MsDocumentReqApplicant DocumentReqApplicant { get; set; }
        public virtual LtDocumentReqStatusWorkflow DocumentReqStatusWorkflow { get; set; }
        public virtual MsStaff Staff { get; set; }
    }

    internal class TrDocumentReqStatusTrackingHistoryConfiguration : AuditEntityConfiguration<TrDocumentReqStatusTrackingHistory>
    {
        public override void Configure(EntityTypeBuilder<TrDocumentReqStatusTrackingHistory> builder)
        {
            builder.Property(x => x.IdDocumentReqApplicant)
                .HasMaxLength(36)
                .IsRequired();

            builder.Property(x => x.IdBinusianStaff)
                .HasMaxLength(36);

            builder.Property(x => x.Remarks)
                .HasMaxLength(500);

            builder.HasOne(x => x.DocumentReqApplicant)
                .WithMany(x => x.DocumentReqStatusTrackingHistories)
                .HasForeignKey(fk => fk.IdDocumentReqApplicant)
                .HasConstraintName("FK_TrDocumentReqStatusTrackingHistory_MsDocumentReqApplicant")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            builder.HasOne(x => x.DocumentReqStatusWorkflow)
                .WithMany(x => x.DocumentReqStatusTrackingHistories)
                .HasForeignKey(fk => fk.IdDocumentReqStatusWorkflow)
                .HasConstraintName("FK_TrDocumentReqStatusTrackingHistory_LtDocumentReqStatusWorkflow")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder.HasOne(x => x.Staff)
                .WithMany(x => x.DocumentReqStatusTrackingHistories)
                .HasForeignKey(fk => fk.IdBinusianStaff)
                .HasConstraintName("FK_TrDocumentReqStatusTrackingHistory_MsStaff")
                .OnDelete(DeleteBehavior.NoAction);

            base.Configure(builder);
        }
    }
}
