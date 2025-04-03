using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.WorkflowDb.Abstractions;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.WorkflowDb.Entities
{
    public class TrApprovalToken : AuditNoUniqueEntity, IWorkflowEntity
    {
        public string IdApprovalToken { get; set; }
        public DateTime ExpiredDate { get; set; }
        public bool StatusToken { get; set; }
        public ApprovalModule Module { get; set; }
        public string IdTransaction { get; set; }
        public DateTime? ActionDate { get; set; }
        public string IpAddress { get; set; }
        public string MacAddress { get; set; }
    }
    internal class TrApprovalTokenConfiguration : AuditNoUniqueEntityConfiguration<TrApprovalToken>
    {
        public override void Configure(EntityTypeBuilder<TrApprovalToken> builder)
        {
            builder.HasKey(x => x.IdApprovalToken);

            builder.Property(x => x.IdApprovalToken)
                .HasMaxLength(36);

            builder.Property(x => x.ExpiredDate)
                .IsRequired();

            builder.Property(x => x.StatusToken)
                .IsRequired();

            builder.Property(x => x.Module)
               .IsRequired();

            builder.Property(x => x.IdTransaction)
                .HasMaxLength(36)
                .IsRequired();

            builder.Property(x => x.IpAddress)
                .HasMaxLength(50);

            builder.Property(x => x.MacAddress)
                .HasMaxLength(50);

            base.Configure(builder);
        }
    }
}
