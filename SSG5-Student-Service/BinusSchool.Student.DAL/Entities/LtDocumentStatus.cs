using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities
{
    public class LtDocumentStatus : AuditEntity, IStudentEntity
    {
        public string DocumentStatusName { get; set; }
        public virtual ICollection<TrStudentDocument> StudentDocument { get; set; }
    }
    internal class LtDocumentStatusConfiguration : AuditEntityConfiguration<LtDocumentStatus>
    {
        public override void Configure(EntityTypeBuilder<LtDocumentStatus> builder)
        {
            builder.Property(x => x.DocumentStatusName)             
                .HasMaxLength(36);

             base.Configure(builder);
        }
    }
}
