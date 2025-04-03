using System;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities
{
    public class TrStudentDocument : AuditEntity, IStudentEntity
    {
        public string IdStudent { get; set; }
        public string IdDocument { get; set; }
        public string FileName { get; set; }
        public decimal FileSize { get; set; }
        public string IdVerificationStatus { get; set; }
        public string Comment { get; set; }
        public string IdDocumentStatus { get; set; }
        public bool isStudentView { get; set; }
        public virtual MsDocument Document { get; set; }
        public virtual LtVerificationStatus VerificationStatus { get; set; }
        public virtual LtDocumentStatus DocumentStatus { get; set; }
    }
    internal class TrStudentDocumentConfiguration : AuditEntityConfiguration<TrStudentDocument>
    {
        public override void Configure(EntityTypeBuilder<TrStudentDocument> builder)
        {
           
            builder.Property(x => x.IdStudent)
                .HasMaxLength(36)
                .IsRequired();

            builder.Property(x => x.FileName)
                .HasMaxLength(100);

            builder.Property(x => x.FileSize) 
                .HasColumnType("DECIMAL(7,2)");     

            builder.Property(x => x.IdDocument)
                .HasMaxLength(36);

            builder.Property(x => x.IdVerificationStatus)
                .HasMaxLength(36);            

            builder.Property(x => x.IdDocumentStatus)
                .HasMaxLength(36);

            builder.Property(x => x.Comment)
                .HasMaxLength(100);      

            builder.HasOne(x => x.Document)
                    .WithMany( y => y.StudentDocument)
                    .HasForeignKey(fk => fk.IdDocument)
                    .HasConstraintName("FK_TrStudentDocument_MsDocument")
                    .OnDelete(DeleteBehavior.NoAction);   

            builder.HasOne(x => x.VerificationStatus)
                    .WithMany( y => y.StudentDocument)
                    .HasForeignKey( fk => fk.IdVerificationStatus)
                    .HasConstraintName("FK_TrStudentDocument_LtVerificationStatus")
                    .OnDelete(DeleteBehavior.Cascade);       

            builder.HasOne(x => x.DocumentStatus)
                    .WithMany( y => y.StudentDocument)
                    .HasForeignKey( fk => fk.IdDocumentStatus)
                    .HasConstraintName("FK_TrStudentDocument_LtDocumentStatus")
                    .OnDelete(DeleteBehavior.Cascade);                 

            base.Configure(builder);
        }
    }
}
