using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.DocumentDb.Abstractions;
using BinusSchool.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.DocumentDb.Entities
{
    public class MsDocumentReqTypeGradeMapping : AuditEntity, IDocumentEntity
    {
        public string CodeGrade { get; set; }
        public string IdDocumentReqType { get; set; }
        public virtual MsDocumentReqType DocumentReqType { get; set; }
    }

    internal class MsDocumentReqTypeGradeMappingConfiguration : AuditEntityConfiguration<MsDocumentReqTypeGradeMapping>
    {
        public override void Configure(EntityTypeBuilder<MsDocumentReqTypeGradeMapping> builder)
        {
            builder.Property(x => x.IdDocumentReqType)
                .HasMaxLength(36)
                .IsRequired();

            builder.Property(x => x.CodeGrade)
                .HasMaxLength(50)
                .IsRequired();

            builder.HasOne(x => x.DocumentReqType)
                .WithMany(x => x.DocumentReqTypeGradeMappings)
                .HasForeignKey(fk => fk.IdDocumentReqType)
                .HasConstraintName("FK_MsDocumentReqTypeGradeMapping_MsDocumentReqType")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
