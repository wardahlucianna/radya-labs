using System.Collections.Generic;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.DocumentDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.DocumentDb.Entities
{
    public class MsFormDoc : AuditEntity, IDocumentEntity
    {
        public string IdForm { get; set; }
        public string JsonDocumentValue { get; set; }
        public string JsonFormElement { get; set; }
        public ApprovalStatus Status { get; set; }

        public virtual MsForm Form { get; set; }
        public virtual ICollection<HMsFormDocApproval> FormDocApprovals { get; set; }
        public virtual ICollection<HMsFormDocChange> FormDocChanges { get; set; }
        public virtual ICollection<HMsFormDocChangeApproval> DocChangeApprovals { get; set; }

    }

    internal class MsFormDocConfiguration : AuditEntityConfiguration<MsFormDoc>
    {
        public override void Configure(EntityTypeBuilder<MsFormDoc> builder)
        {
           
            builder.Property(x => x.JsonDocumentValue)
                .HasColumnType("text")
                .IsRequired();
                
            builder.Property(x => x.JsonFormElement)
                .HasColumnType("text")
                .IsRequired();

            builder.HasOne(x => x.Form)
                .WithMany(x => x.FormDocs)
                .HasForeignKey(fk => fk.IdForm)
                .HasConstraintName("FK_MsFormDoc_MsForm")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
