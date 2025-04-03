using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Persistence.DocumentDb.Abstractions;
using BinusSchool.Persistence.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.DocumentDb.Entities
{
    public class LtDocumentReqStatusWorkflow : AuditNoUniqueEntity, IDocumentEntity
    {
        public DocumentRequestStatusWorkflow IdDocumentReqStatusWorkflow { get; set; }
        public string Code { get; set; }
        public string StaffDescription { get; set; }
        public string ParentDescription { get; set; }
        public virtual ICollection<TrDocumentReqStatusTrackingHistory> DocumentReqStatusTrackingHistories { get; set; }
        public virtual ICollection<MsDocumentReqApplicant> DocumentReqApplicants { get; set; }
    }

    internal class LtDocumentReqStatusWorkflowConfiguration : AuditNoUniqueEntityConfiguration<LtDocumentReqStatusWorkflow>
    {
        public override void Configure(EntityTypeBuilder<LtDocumentReqStatusWorkflow> builder)
        {
            builder.HasKey(x => x.IdDocumentReqStatusWorkflow);

            builder.Property(x => x.Code)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(x => x.StaffDescription)
                .HasMaxLength(128)
                .IsRequired();

            builder.Property(x => x.ParentDescription)
                .HasMaxLength(128)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
