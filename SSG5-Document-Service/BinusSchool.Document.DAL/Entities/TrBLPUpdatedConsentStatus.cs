using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.DocumentDb.Abstractions;
using BinusSchool.Persistence.DocumentDb.Entities.Scheduling;
using BinusSchool.Persistence.DocumentDb.Entities.Student;
using BinusSchool.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.DocumentDb.Entities
{
    public class TrBLPUpdatedConsentStatus : AuditEntity, IDocumentEntity
    {
        public string IdStudent { get; set; }
        public string IdHomeroomStudent { get; set; }
        public string IdBLPStatusBefore { get; set; }
        public string IdBLPStatusAfter { get; set; }
        public bool HasSentEmailToStaff { get; set; }
        public DateTime TransactionDate { get; set; }
        public virtual MsStudent Student { get; set; }
        public virtual MsHomeroomStudent HomeroomStudent { get; set; }
        public virtual LtBLPStatus BeforemBLPStatus { get; set; }
        public virtual LtBLPStatus AfterBLPStatus { get; set; }
    }

    internal class TrBLPUpdatedConsentStatusConfiguration : AuditEntityConfiguration<TrBLPUpdatedConsentStatus>
    {
        public override void Configure(EntityTypeBuilder<TrBLPUpdatedConsentStatus> builder)
        {
            builder.Property(x => x.IdStudent)
                .HasMaxLength(36)
                .IsRequired();

            builder.Property(x => x.IdHomeroomStudent)
                .HasMaxLength(36)
                .IsRequired();

            builder.Property(x => x.IdBLPStatusBefore)
                .HasMaxLength(36);

            builder.Property(x => x.IdBLPStatusAfter)
                .HasMaxLength(36)
                .IsRequired();

            builder.Property(x => x.TransactionDate)
                .IsRequired();

            builder.Property(x => x.HasSentEmailToStaff)
                .IsRequired();

            builder.HasOne(x => x.Student)
                .WithMany(x => x.BLPUpdatedConsentStatuses)
                .HasForeignKey(fk => fk.IdStudent)
                .HasConstraintName("FK_TrBLPUpdatedConsentStatus_MsStudent")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            builder.HasOne(x => x.HomeroomStudent)
                .WithMany(x => x.BLPUpdatedConsentStatuses)
                .HasForeignKey(fk => fk.IdHomeroomStudent)
                .HasConstraintName("FK_TrBLPUpdatedConsentStatus_MsHomeroomStudent")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            builder.HasOne(x => x.BeforemBLPStatus)
                .WithMany(x => x.BeforeBLPUpdatedConsentStatuses)
                .HasForeignKey(fk => fk.IdBLPStatusBefore)
                .HasConstraintName("FK_TrBLPUpdatedConsentStatus_LtBLPStatus_Before")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder.HasOne(x => x.AfterBLPStatus)
                .WithMany(x => x.AfterBLPUpdatedConsentStatuses)
                .HasForeignKey(fk => fk.IdBLPStatusAfter)
                .HasConstraintName("FK_TrBLPUpdatedConsentStatus_LtBLPStatus_After")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
