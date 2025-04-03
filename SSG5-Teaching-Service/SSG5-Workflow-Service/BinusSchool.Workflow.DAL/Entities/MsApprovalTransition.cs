using BinusSchool.Common.Model.Enums;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.WorkflowDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.WorkflowDb.Entities
{
    public class MsApprovalTransition : AuditEntity, IWorkflowEntity
    {
        public string IdToState { get; set; }
        public string IdFromState { get; set; }
        public ApprovalAction Action { get; set; }
        public ApprovalStatus Status { get; set; }

        public virtual MsApprovalState ToState { get; set; }
        public virtual MsApprovalState FromState { get; set; }
    }

    internal class MsApprovalTransitionConfiguration : AuditEntityConfiguration<MsApprovalTransition>
    {
        public override void Configure(EntityTypeBuilder<MsApprovalTransition> builder)
        {
            builder.Property(x => x.Action)
                .IsRequired();

            builder.Property(x => x.Status)
                .IsRequired();

            builder.HasOne(x => x.ToState)
                .WithMany(x => x.ToTransitions)
                .HasForeignKey(fk => fk.IdToState)
                .HasConstraintName("FK_MsApprovalTransition_MsApprovalState_To")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder.HasOne(x => x.FromState)
                .WithMany(x => x.FromTransitions)
                .HasForeignKey(fk => fk.IdFromState)
                .HasConstraintName("FK_MsApprovalTransition_MsApprovalState_From")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
