using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.DocumentDb.Abstractions;
using BinusSchool.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.DocumentDb.Entities
{
    public class TrDocumentReqPIC : AuditEntity, IDocumentEntity
    {
        public string IdDocumentReqApplicantDetail { get; set; }
        public string IdBinusian { get; set; }
        public virtual TrDocumentReqApplicantDetail DocumentReqApplicantDetail { get; set; }
        public virtual MsStaff Staff { get; set; }
    }

    internal class TrDocumentReqPICConfiguration : AuditEntityConfiguration<TrDocumentReqPIC>
    {
        public override void Configure(EntityTypeBuilder<TrDocumentReqPIC> builder)
        {
            builder.Property(x => x.IdDocumentReqApplicantDetail)
                .HasMaxLength(36)
                .IsRequired();

            builder.Property(x => x.IdBinusian)
                .HasMaxLength(36)
                .IsRequired();

            builder.HasOne(x => x.DocumentReqApplicantDetail)
                .WithMany(x => x.DocumentReqPICs)
                .HasForeignKey(fk => fk.IdDocumentReqApplicantDetail)
                .HasConstraintName("FK_TrDocumentReqPIC_TrDocumentReqApplicantDetail")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            builder.HasOne(x => x.Staff)
                .WithMany(x => x.DocumentReqPICs)
                .HasForeignKey(fk => fk.IdBinusian)
                .HasConstraintName("FK_TrDocumentReqPIC_MsStaff")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
