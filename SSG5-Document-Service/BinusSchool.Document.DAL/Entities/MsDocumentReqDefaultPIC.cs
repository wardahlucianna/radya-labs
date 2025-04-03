using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.DocumentDb.Abstractions;
using BinusSchool.Persistence.DocumentDb.Entities;
using BinusSchool.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.DocumentDb.Entities
{
    public class MsDocumentReqDefaultPIC : AuditEntity, IDocumentEntity
    {
        public string IdDocumentReqType { get; set; }
        public string IdBinusian { get; set; }
        public virtual MsDocumentReqType DocumentReqType { get; set; }
        public virtual MsStaff Staff { get; set; }
    }

    internal class MsDocumentReqDefaultPICConfiguration : AuditEntityConfiguration<MsDocumentReqDefaultPIC>
    {
        public override void Configure(EntityTypeBuilder<MsDocumentReqDefaultPIC> builder)
        {
            builder.Property(x => x.IdDocumentReqType)
                .HasMaxLength(36)
                .IsRequired();

            builder.Property(x => x.IdBinusian)
                .HasMaxLength(36)
                .IsRequired();

            builder.HasOne(x => x.DocumentReqType)
                .WithMany(x => x.DocumentReqDefaultPICs)
                .HasForeignKey(fk => fk.IdDocumentReqType)
                .HasConstraintName("FK_MsDocumentReqDefaultPIC_MsDocumentReqType")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            builder.HasOne(x => x.Staff)
                .WithMany(x => x.DocumentReqDefaultPICs)
                .HasForeignKey(fk => fk.IdBinusian)
                .HasConstraintName("FK_MsDocumentReqDefaultPIC_MsStaff")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
