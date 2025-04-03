using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities
{
    public class TrMedicalDocument : AuditEntity, IStudentEntity
    {
        public string MedicalDocumentName { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public string IdUser { get; set; }
    }

    internal class TrMedicalDocumentConfiguration : AuditEntityConfiguration<TrMedicalDocument>
    {
        public override void Configure(EntityTypeBuilder<TrMedicalDocument> builder)
        {
            builder.Property(x => x.MedicalDocumentName)
                .HasMaxLength(100);

            builder.Property(x => x.FileName)
                .HasMaxLength(500)
                .IsRequired();

            builder.Property(x => x.FilePath)
                .HasMaxLength(500)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
