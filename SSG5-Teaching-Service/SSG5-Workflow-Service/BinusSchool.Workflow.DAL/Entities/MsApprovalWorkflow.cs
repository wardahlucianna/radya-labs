using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.WorkflowDb.Abstractions;
using BinusSchool.Persistence.WorkflowDb.Entities.School;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.WorkflowDb.Entities
{
    public class MsApprovalWorkflow : CodeEntity, IWorkflowEntity
    {
        public string IdSchool { get; set; }
        public virtual MsSchool School { get; set; }
        public virtual ICollection<MsApprovalState> ApprovalStates { get; set; }
    }

    internal class MsApprovalWorkflowConfiguration : CodeEntityConfiguration<MsApprovalWorkflow>
    {
        public override void Configure(EntityTypeBuilder<MsApprovalWorkflow> builder)
        {
            base.Configure(builder);

            builder.Property(x => x.IdSchool)
                .HasMaxLength(36)
               .IsRequired();

            builder.HasOne(x => x.School)
             .WithMany(x => x.ApprovalWorkflows)
             .HasForeignKey(fk => fk.IdSchool)
             .HasConstraintName("FK_MsApprovalWorkflow_MsSchool")
             .OnDelete(DeleteBehavior.NoAction)
               .IsRequired();

            builder.Property(x => x.Description)
                .IsRequired(false);
        }
    }
}
