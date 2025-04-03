using BinusSchool.Common.Model.Enums;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.WorkflowDb.Abstractions;
using BinusSchool.Persistence.WorkflowDb.Entities.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.WorkflowDb.Entities
{
    public class MsApprovalHistory : AuditEntity, IWorkflowEntity
    {
        public string IdDocument { get; set; }
        public string IdFormState { get; set; }
        public string IdUserAction { get; set; }
        public ApprovalStatus Action { get; set; }
        public MsUser UserAction { get; set; }
    }

    internal class MsApprovalHistoryConfiguration : AuditEntityConfiguration<MsApprovalHistory>
    {
        public override void Configure(EntityTypeBuilder<MsApprovalHistory> builder)
        {
            builder.Property(x => x.IdDocument)
                .HasMaxLength(36)
                .IsRequired();
                
            builder.Property(x => x.IdFormState)
                .HasMaxLength(36)
                .IsRequired();
                
            builder.Property(x => x.IdUserAction)
                .HasMaxLength(36)
                .IsRequired();

            builder.Property(x => x.Action)
                .IsRequired();

            builder.HasOne(x => x.UserAction)
              .WithMany(x => x.ApprovalHistorys)
              .HasForeignKey(fk => fk.IdUserAction)
              .HasConstraintName("FK_MsApprovalHistory_MsUser")
              .OnDelete(DeleteBehavior.Cascade)
               .IsRequired();

            base.Configure(builder);
        }
    }
}
