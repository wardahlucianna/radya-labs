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
    public class MsCountryFact :  AuditEntity, IStudentEntity
    {
        public string IdAcademicYear { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Website { get; set; }
        public string ContactPerson { get; set; }
        public virtual MsAcademicYear AcademicYear { get; set; }
        public virtual ICollection<MsCountryFactLevel> CountryFactLevel { get; set; }
        public virtual ICollection<MsCountryFactSheet> CountryFactSheet { get; set; }
        public virtual ICollection<MsCountryFactLogo> CountryFactLogo { get; set; }
    }

    internal class MsCountryFactConfiguration : AuditEntityConfiguration<MsCountryFact>
    {
        public override void Configure(EntityTypeBuilder<MsCountryFact> builder)
        {
            builder.Property(x => x.Name).IsRequired().HasMaxLength(100);
            builder.Property(x => x.Description).HasMaxLength(450);
            builder.Property(x => x.ContactPerson).HasMaxLength(30);
            builder.Property(x => x.Website).HasMaxLength(50);

            builder.HasOne(x => x.AcademicYear)
             .WithMany(x => x.CountryFact)
             .HasForeignKey(fk => fk.IdAcademicYear)
             .HasConstraintName("FK_MsCountryFact_MsAcademicYear")
             .OnDelete(DeleteBehavior.Restrict)
             .IsRequired();

            base.Configure(builder);
        }
    }
}
