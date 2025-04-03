using System;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.DocumentDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BinusSchool.Persistence.DocumentDb.Entities.User;

namespace BinusSchool.Persistence.DocumentDb.Entities
{
    public class HMsFormDocChangeApproval : AuditEntity, IDocumentEntity
    {
        public string IdsFormDocChange { get; set; }
        public string IdUserActionBy { get; set; }
        public string IdFormDocument { get; set; }
        public string IdRoleActionBy { get; set; }
        public string IdForRoleActionNext { get; set; }
        public string Comment { get; set; }
        public ApprovalStatus Action { get; set; }
        public DateTime ActionDate { get; set; }
        public string IdState { get; set; }

        public virtual HMsFormDocChange FormDocChange { get; set; }
        public virtual MsFormDoc FormDoc { get; set; }
        public virtual MsUser User { get; set; }
        public virtual LtRole Role { get; set; }
        public virtual LtRole RoleActionNext { get; set; }
    }

    internal class HsFormDocChangeApprovalConfiguration : AuditEntityConfiguration<HMsFormDocChangeApproval>
    {
        public override void Configure(EntityTypeBuilder<HMsFormDocChangeApproval> builder)
        {
            builder.Property(x => x.IdUserActionBy)
                .HasMaxLength(36)
                .IsRequired();

            builder.Property(x => x.IdFormDocument)
                .HasMaxLength(36)
                .IsRequired();

            builder.Property(x => x.IdRoleActionBy)
                .HasMaxLength(36)
                .IsRequired();

            builder.Property(x => x.IdForRoleActionNext)
                .HasMaxLength(36);

            builder.Property(x => x.Action)
                .IsRequired();
            
            builder.Property(x => x.ActionDate)
                .IsRequired();

            builder.HasOne(x => x.FormDocChange)
                .WithMany(x => x.DocChangeApprovals)
                .HasForeignKey(fk => fk.IdsFormDocChange)
                .HasConstraintName("FK_HsFormDocChangeApproval_HsFormDocChange")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder.HasOne(x => x.FormDoc)
               .WithMany(x => x.DocChangeApprovals)
               .HasForeignKey(fk => fk.IdFormDocument)
               .HasConstraintName("FK_HsFormDocChangeApproval_MsFormDoc")
               .OnDelete(DeleteBehavior.Cascade)
               .IsRequired();

            builder.HasOne(x => x.User)
                .WithMany(x => x.DocChangeApprovals)
                .HasForeignKey(fk => fk.IdUserActionBy)
                .HasConstraintName("FK_HsFormDocChangeApproval_MsUser")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            builder.HasOne(x => x.Role)
                .WithMany(x => x.DocChangeApprovals)
                .HasForeignKey(fk => fk.IdRoleActionBy)
                .HasConstraintName("FK_HsFormDocChangeApproval_MsRole")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder.HasOne(x => x.RoleActionNext)
             .WithMany(x => x.DocChangeApprovalsActionNext)
             .HasForeignKey(fk => fk.IdForRoleActionNext)
             .HasConstraintName("FK_HsFormDocChangeApproval_MsRoleActionNext")
             .OnDelete(DeleteBehavior.NoAction)
             .IsRequired();

            builder.Property(x => x.IdState)
               .HasMaxLength(36);

            base.Configure(builder);
        }
    }
}
