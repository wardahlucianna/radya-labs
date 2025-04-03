using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities.School;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities
{
    public class TrStudentPhoto : AuditEntity, IStudentEntity
    {
        public string IdAcademicYear { get; set; }
        public string IdStudent { get; set; }
        public string IdBinusian { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }

        public virtual MsStudent Student { get; set; }
        public virtual MsAcademicYear AcademicYear { get; set; }
    }

    internal class TrStudentPhotoConfiguration : AuditEntityConfiguration<TrStudentPhoto>
    {
        public override void Configure(EntityTypeBuilder<TrStudentPhoto> builder)
        {
            builder.Property(x => x.FileName)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(x => x.FilePath)
                .HasMaxLength(150)
                .IsRequired();

            builder.Property(x => x.IdBinusian)
                .HasMaxLength(36)
                .IsRequired();

            builder.HasOne(x => x.Student)
              .WithMany(x => x.StudentPhotos)
              .HasForeignKey(fk => fk.IdStudent)
              .HasConstraintName("FK_TrStudentPhoto_MsStudent")
              .OnDelete(DeleteBehavior.NoAction)
              .IsRequired();

            builder.HasOne(x => x.AcademicYear)
              .WithMany(x => x.StudentPhotos)
              .HasForeignKey(fk => fk.IdAcademicYear)
              .HasConstraintName("FK_TrStudentPhoto_MsAcademicYear")
              .OnDelete(DeleteBehavior.NoAction)
              .IsRequired();

            base.Configure(builder);
        }
    }
}
