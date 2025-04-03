using BinusSchool.Common.Model.Enums;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.DocumentDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BinusSchool.Persistence.DocumentDb.Entities.User;

namespace BinusSchool.Persistence.DocumentDb.Entities
{
    public class HMsFormDocApproval : AuditEntity, IDocumentEntity
    {
        public string IdFormDoc { get; set; }
        public string IdUserActionBy { get; set; }
        public string IdRoleActionBy { get; set; }
        public string IdForRoleActionNext { get; set; }
        public string Comment { get; set; }
        public ApprovalStatus Action { get; set; }
        public string IdState { get; set; }

        public virtual MsFormDoc FormDoc { get; set; }
        public virtual MsUser User { get; set; }
        public virtual LtRole Role { get; set; }
        public virtual LtRole RoleActionNext { get; set; }
    }

    internal class HsFormDocApprovalConfiguration : AuditEntityConfiguration<HMsFormDocApproval>
    {
        public override void Configure(EntityTypeBuilder<HMsFormDocApproval> builder)
        {
            builder.Property(x => x.IdUserActionBy)
                .HasMaxLength(36)
                .IsRequired();

            builder.Property(x => x.IdRoleActionBy)
                .HasMaxLength(36)
                .IsRequired();
                
            builder.Property(x => x.IdForRoleActionNext)
                .HasMaxLength(36)
                .IsRequired();

            builder.Property(x => x.Action)
                .IsRequired();

            builder.Property(x => x.IdState)
                .HasMaxLength(36)
                .IsRequired();

            builder.HasOne(x => x.FormDoc)
                .WithMany(x => x.FormDocApprovals)
                .HasForeignKey(fk => fk.IdFormDoc)
                .HasConstraintName("FK_HsFormDocApproval_MsFormDoc")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            builder.HasOne(x => x.User)
                .WithMany(x => x.FormDocApprovals)
                .HasForeignKey(fk => fk.IdUserActionBy)
                .HasConstraintName("FK_HsFormDocApproval_MsUser")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            builder.HasOne(x => x.Role)
                .WithMany(x => x.FormDocApprovals)
                .HasForeignKey(fk => fk.IdRoleActionBy)
                .HasConstraintName("FK_HsFormDocApproval_MsRole")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder.HasOne(x => x.RoleActionNext)
             .WithMany(x => x.FormDocApprovalsActionNext)
             .HasForeignKey(fk => fk.IdForRoleActionNext)
             .HasConstraintName("FK_HsFormDocApproval_MsRoleActionNext")
             .OnDelete(DeleteBehavior.NoAction)
             .IsRequired();

            base.Configure(builder);
        }
    }
}
