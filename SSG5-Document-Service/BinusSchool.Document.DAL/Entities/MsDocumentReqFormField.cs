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
    public class MsDocumentReqFormField : AuditEntity, IDocumentEntity
    {
        public string IdDocumentReqType { get; set; }
        public string IdDocumentReqFieldType { get; set; }
        public string QuestionDescription { get; set; }
        public int OrderNumber { get; set; }
        public bool IsRequired { get; set; }
        public string IdDocumentReqOptionCategory { get; set; }
        public virtual MsDocumentReqType DocumentReqType { get; set; }
        public virtual LtDocumentReqFieldType DocumentReqFieldType { get; set; }
        public virtual MsDocumentReqOptionCategory DocumentReqOptionCategory { get; set; }
    }

    internal class MsDocumentReqFormFieldConfiguration : AuditEntityConfiguration<MsDocumentReqFormField>
    {
        public override void Configure(EntityTypeBuilder<MsDocumentReqFormField> builder)
        {
            builder.Property(x => x.IdDocumentReqType)
                .HasMaxLength(36)
                .IsRequired();

            builder.Property(x => x.QuestionDescription)
               .HasMaxLength(128)
               .IsRequired();

            builder.Property(x => x.IdDocumentReqFieldType)
                .HasMaxLength(36)
                .IsRequired();

            builder.Property(x => x.IdDocumentReqOptionCategory)
                .HasMaxLength(36);

            builder.HasOne(x => x.DocumentReqType)
                .WithMany(x => x.DocumentReqFormFields)
                .HasForeignKey(fk => fk.IdDocumentReqType)
                .HasConstraintName("FK_MsDocumentReqFormField_MsDocumentReqType")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            builder.HasOne(x => x.DocumentReqFieldType)
                .WithMany(x => x.DocumentReqFormFields)
                .HasForeignKey(fk => fk.IdDocumentReqFieldType)
                .HasConstraintName("FK_MsDocumentReqFormField_LtDocumentReqFieldType")
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            builder.HasOne(x => x.DocumentReqOptionCategory)
                .WithMany(x => x.DocumentReqFormFields)
                .HasForeignKey(fk => fk.IdDocumentReqOptionCategory)
                .HasConstraintName("FK_MsDocumentReqFormField_MsDocumentReqOption")
                .OnDelete(DeleteBehavior.NoAction);

            base.Configure(builder);
        }
    }
}
