using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities
{
    public class LtDocumentType : AuditEntity, IStudentEntity
    {
        public string DocumentTypeName { get; set; }
        public virtual ICollection<MsDocument> Document { get; set; }
    }
    internal class LtDocumentTypeConfiguration : AuditEntityConfiguration<LtDocumentType>
    {
        public override void Configure(EntityTypeBuilder<LtDocumentType> builder)
        {
             builder.Property(x => x.DocumentTypeName)
                .HasMaxLength(80);

            base.Configure(builder);
        }

    }
}
