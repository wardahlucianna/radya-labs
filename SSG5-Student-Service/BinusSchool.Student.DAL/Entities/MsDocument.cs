using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities
{
    public class MsDocument : AuditEntity, IStudentEntity
    {
        public string DocumentName { get; set; }
        public string DocumentNameEng { get; set; }
        public string IdDocumentType { get; set; }
        public virtual ICollection<TrStudentDocument> StudentDocument { get; set; }
        public virtual LtDocumentType DocumentType { get; set; }
    }
    internal class MsDocumentConfiguration : AuditEntityConfiguration<MsDocument>
    {
        public override void Configure(EntityTypeBuilder<MsDocument> builder)
        {
            builder.Property(x => x.DocumentName)
                .HasMaxLength(250);

            builder.Property(x => x.DocumentNameEng)
                .HasMaxLength(250);

            builder.Property(x => x.IdDocumentType)
                .HasMaxLength(36);    
        
            builder.HasOne(x => x.DocumentType)
                    .WithMany( y => y.Document)
                    .HasForeignKey( fk => fk.IdDocumentType)
                    .HasConstraintName("FK_MsDocument_LtDocumentType")
                    .OnDelete(DeleteBehavior.Cascade);

            base.Configure(builder);
        }
    }
}
