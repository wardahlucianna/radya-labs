using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.DocumentDb.Abstractions;
using BinusSchool.Persistence.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.DocumentDb.Entities
{
    public class LtDocumentReqFieldType : AuditEntity, IDocumentEntity
    {
        public string Type { get; set; }
        public bool HasOption { get; set; }
        public virtual ICollection<MsDocumentReqFormField> DocumentReqFormFields { get; set; }
        public virtual ICollection<MsDocumentReqOptionCategory> DocumentReqOptionCategories { get; set; }
        public virtual ICollection<MsDocumentReqFormFieldAnswered> DocumentReqFormFieldAnswereds { get; set; }
    }

    internal class LtDocumentReqFieldTypeConfiguration : AuditEntityConfiguration<LtDocumentReqFieldType>
    {
        public override void Configure(EntityTypeBuilder<LtDocumentReqFieldType> builder)
        {
            builder.Property(x => x.Type)
                .HasMaxLength(50)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
