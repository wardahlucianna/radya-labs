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
    public class MsGcLinkLogo : AuditEntity, IStudentEntity
    {
        public string IdGcLink { get; set; }
        public string OriginalName { get; set; }
        public int FileSize { get; set; }
        public string FileName { get; set; }
        public string Url { get; set; }
        public string FileType { get; set; }
        public virtual MsGcLink GcLink { get; set; }

    }

    internal class MsGcLinkLogoConfiguration : AuditEntityConfiguration<MsGcLinkLogo>
    {
        public override void Configure(EntityTypeBuilder<MsGcLinkLogo> builder)
        {
            builder.Property(p => p.Url).HasMaxLength(450);
            builder.Property(p => p.OriginalName).HasMaxLength(100);
            builder.Property(p => p.FileName).HasMaxLength(100);
            builder.Property(p => p.FileType).HasMaxLength(10);
            builder.Property(x => x.FileSize).HasColumnType("decimal(18,2)");

            builder.HasOne(x => x.GcLink)
             .WithMany(x => x.GcLinkLogo)
             .HasForeignKey(fk => fk.IdGcLink)
             .HasConstraintName("FK_MsGcLinkLogo_MsGcLink")
             .OnDelete(DeleteBehavior.Restrict)
             .IsRequired();

            base.Configure(builder);
        }
    }
}
