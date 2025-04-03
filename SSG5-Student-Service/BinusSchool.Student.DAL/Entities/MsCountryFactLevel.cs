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
    public class MsCountryFactLevel : AuditEntity, IStudentEntity
    {
        public string IdCountryFact { get; set; }
        public string IdLevel { get; set; }
        public virtual MsCountryFact CountryFact { get; set; }
        public virtual MsLevel Level { get; set; }

    }

    internal class MsCountryFactLevelConfiguration : AuditEntityConfiguration<MsCountryFactLevel>
    {
        public override void Configure(EntityTypeBuilder<MsCountryFactLevel> builder)
        {
            builder.HasOne(x => x.CountryFact)
             .WithMany(x => x.CountryFactLevel)
             .HasForeignKey(fk => fk.IdCountryFact)
             .HasConstraintName("FK_MsCountryFactLevel_MsCountryFact")
             .OnDelete(DeleteBehavior.Restrict)
             .IsRequired();

            builder.HasOne(x => x.Level)
             .WithMany(x => x.CountryFactLevel)
             .HasForeignKey(fk => fk.IdLevel)
             .HasConstraintName("FK_MsCountryFactLevel_MsLevel")
             .OnDelete(DeleteBehavior.Restrict)
             .IsRequired();

            base.Configure(builder);
        }
    }
}
