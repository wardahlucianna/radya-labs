using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities.School;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities
{
    public class MsUniversityPortalLogo : AuditEntity, IStudentEntity
    {
        public string IdUniversityPortal { get; set; }
        public string OriginalName { get; set; }
        public int FileSize { get; set; }
        public string FileName { get; set; }
        public string Url { get; set; }
        public string FileType { get; set; }

        public virtual MsUniversityPortal UniversityPortal { get; set; }
    }

    internal class MsUniversityPortalLogoConfiguration : AuditEntityConfiguration<MsUniversityPortalLogo>
    {
        public override void Configure(EntityTypeBuilder<MsUniversityPortalLogo> builder)
        {
            builder.Property(p => p.Url).HasMaxLength(450);
            builder.Property(p => p.OriginalName).HasMaxLength(100);
            builder.Property(p => p.FileName).HasMaxLength(100);
            builder.Property(p => p.FileType).HasMaxLength(10);
            builder.Property(x => x.FileSize).HasColumnType("decimal(18,2)");

            builder.HasOne(x => x.UniversityPortal)
             .WithMany(x => x.UniversityPortalLogo)
             .HasForeignKey(fk => fk.IdUniversityPortal)
             .HasConstraintName("FK_MsUniversityPortalLogo_MsUniversityPortal")
             .OnDelete(DeleteBehavior.Restrict)
             .IsRequired();

            base.Configure(builder);
        }
    }
}
