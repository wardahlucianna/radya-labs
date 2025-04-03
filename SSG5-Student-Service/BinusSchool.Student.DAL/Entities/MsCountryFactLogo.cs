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
    public class MsCountryFactLogo : AuditEntity, IStudentEntity
    {
        public string IdCountryFact { get; set; }
        public string OriginalName { get; set; }
        public int FileSize { get; set; }
        public string FileName { get; set; }
        public string Url { get; set; }
        public string FileType { get; set; }
        public virtual MsCountryFact CountryFact { get; set; }

    }

    internal class MsCountryFactLogoConfiguration : AuditEntityConfiguration<MsCountryFactLogo>
    {
        public override void Configure(EntityTypeBuilder<MsCountryFactLogo> builder)
        {
            builder.Property(p => p.Url).HasMaxLength(450);
            builder.Property(p => p.OriginalName).HasMaxLength(100);
            builder.Property(p => p.FileName).HasMaxLength(100);
            builder.Property(p => p.FileType).HasMaxLength(10);
            builder.Property(x => x.FileSize).HasColumnType("decimal(18,2)");

            builder.HasOne(x => x.CountryFact)
             .WithMany(x => x.CountryFactLogo)
             .HasForeignKey(fk => fk.IdCountryFact)
             .HasConstraintName("FK_MsCountryFactLogo_MsCountryFact")
             .OnDelete(DeleteBehavior.Restrict)
             .IsRequired();

            base.Configure(builder);
        }
    }
}
