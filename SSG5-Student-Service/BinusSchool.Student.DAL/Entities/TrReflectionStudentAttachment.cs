using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities
{
    public class TrReflectionStudentAttachment : AuditEntity, IStudentEntity
    {
        public string IdReflectionStudent { get; set; }
        public string Url { get; set; }
        public string FileName { get; set; }
        public string FileNameOriginal { get; set; }
        public string FileType { get; set; }
        public decimal FileSize { get; set; }

        public virtual TrReflectionStudent ReflectionStudent { get; set; }
    }

    internal class TrReflectionStudentAttachmentConfiguration : AuditEntityConfiguration<TrReflectionStudentAttachment>
    {
        public override void Configure(EntityTypeBuilder<TrReflectionStudentAttachment> builder)
        {
            builder.Property(x => x.Url)
               .HasMaxLength(450)
               .IsRequired();

            builder.Property(x => x.FileName)
               .HasMaxLength(200)
               .IsRequired();

            builder.Property(x => x.FileNameOriginal)
               .HasMaxLength(200)
               .IsRequired();

            builder.Property(x => x.FileType)
               .HasMaxLength(10)
               .IsRequired();

            builder.Property(x => x.FileSize)
               .IsRequired();

            builder.HasOne(x => x.ReflectionStudent)
                .WithMany(x => x.ReflectionStudentAttachments)
                .HasForeignKey(fk => fk.IdReflectionStudent)
                .HasConstraintName("FK_TrReflectionStudentAttachment_TrReflectionStudent")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
