using System.Collections.Generic;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.DocumentDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BinusSchool.Persistence.DocumentDb.Entities.User;

namespace BinusSchool.Persistence.DocumentDb.Entities
{
    public class HMsFormDocChange : AuditEntity, IDocumentEntity
    {
        public string IdFormDoc { get; set; }
        public string IdUserExecutor { get; set; }
        public string JsonDocumentValueOld { get; set; }
        public string JsonDocumentValueNew { get; set; }
        public string JsonFormElementOld { get; set; }
        public string JsonFormElementNew { get; set; }
        public int Order { get; set; }
        public ApprovalStatus Status { get; set; }

        public virtual MsUser User { get; set; }
        public virtual MsFormDoc FormDoc { get; set; }
        public virtual ICollection<HMsFormDocChangeApproval> DocChangeApprovals { get; set; }
        public virtual ICollection<HMsFormDocChangeNote> DocChangeNotes { get; set; }
    }

    internal class HsFormDocChangeConfiguration : AuditEntityConfiguration<HMsFormDocChange>
    {
        public override void Configure(EntityTypeBuilder<HMsFormDocChange> builder)
        {
            builder.Property(x => x.JsonDocumentValueOld)
                .HasColumnType("text")
                .IsRequired();
                
            builder.Property(x => x.JsonDocumentValueNew)
                .HasColumnType("text")
                .IsRequired();

            builder.Property(x => x.JsonFormElementOld)
                .HasColumnType("text")
                .IsRequired();

            builder.Property(x => x.JsonFormElementNew)
                .HasColumnType("text")
                .IsRequired();

            builder.Property(x => x.Status)
                .IsRequired();

            builder.HasOne(x => x.FormDoc)
                .WithMany(x => x.FormDocChanges)
                .HasForeignKey(fk => fk.IdFormDoc)
                .HasConstraintName("FK_HsFormDocChange_MsFormDoc")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            builder.HasOne(x => x.User)
                .WithMany(x => x.FormDocChanges)
                .HasForeignKey(fk => fk.IdUserExecutor)
                .HasConstraintName("FK_HsFormDocChange_MsUser")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            builder.Property(x => x.IdUserExecutor).HasMaxLength(36);

            base.Configure(builder);
        }
    }
}
