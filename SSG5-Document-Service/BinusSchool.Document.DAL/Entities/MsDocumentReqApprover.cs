using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.DocumentDb.Abstractions;
using BinusSchool.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.DocumentDb.Entities
{
    public class MsDocumentReqApprover : AuditEntity, IDocumentEntity
    {
        public string IdSchool { get; set; }
        public string IdBinusian { get; set; }
        public virtual MsSchool School { get; set; }
        public virtual MsStaff Staff { get; set; }
    }

    internal class MsDocumentReqApproverConfiguration : AuditEntityConfiguration<MsDocumentReqApprover>
    {
        public override void Configure(EntityTypeBuilder<MsDocumentReqApprover> builder)
        {
            builder.Property(x => x.IdSchool)
                .HasMaxLength(36)
                .IsRequired();

            builder.Property(x => x.IdBinusian)
                .HasMaxLength(36)
                .IsRequired();

            builder.HasOne(x => x.School)
                .WithMany(x => x.DocumentReqApprovers)
                .HasForeignKey(fk => fk.IdSchool)
                .HasConstraintName("FK_MsDocumentReqApprover_MsSchool")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder.HasOne(x => x.Staff)
                .WithMany(x => x.DocumentReqApprovers)
                .HasForeignKey(fk => fk.IdBinusian)
                .HasConstraintName("FK_MsDocumentReqApprover_MsStaff")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
