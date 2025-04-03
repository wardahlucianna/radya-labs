using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.WorkflowDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.WorkflowDb.Entities
{
    public class MsApprovalState : AuditEntity, IWorkflowEntity
    {
        public string IdApprovalWorkflow { get; set; }
        public string IdRole { get; set; }
        public string StateName { get; set; }
        public string StateType { get; set; }
        public int StateNumber { get; set; }

        public virtual MsApprovalWorkflow ApprovalWorkflow { get; set; }
        public virtual ICollection<MsApprovalTransition> ToTransitions { get; set; }
        public virtual ICollection<MsApprovalTransition> FromTransitions { get; set; }
    }

    internal class MsApprovalStateConfiguration : AuditEntityConfiguration<MsApprovalState>
    {
        public override void Configure(EntityTypeBuilder<MsApprovalState> builder)
        {
            builder.Property(x => x.IdRole)
                .HasMaxLength(36);

            builder.Property(x => x.StateName)
                .HasMaxLength(32)
                .IsRequired();

            builder.Property(x => x.StateType)
                .HasMaxLength(32)
                .IsRequired();

            builder.Property(x => x.StateNumber)
                .IsRequired();

            builder.HasOne(x => x.ApprovalWorkflow)
                .WithMany(x => x.ApprovalStates)
                .HasForeignKey(fk => fk.IdApprovalWorkflow)
                .HasConstraintName("FK_MsApprovalState_MsApprovalWorkflow")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
