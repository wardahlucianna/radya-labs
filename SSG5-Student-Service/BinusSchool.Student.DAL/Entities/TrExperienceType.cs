using System;
using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities.School;
using BinusSchool.Persistence.StudentDb.Entities.Scheduling;
using BinusSchool.Persistence.StudentDb.Entities.User;
using BinusSchool.Common.Model.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities
{
    public class TrExperienceType : AuditEntity, IStudentEntity
    {
        public string IdExperience { get; set; }
        public ExperienceType ExperienceType { get; set; }
        public virtual TrExperience Experience { get; set; }
    }

    internal class TrExperienceTypeConfiguration : AuditEntityConfiguration<TrExperienceType>
    {
        public override void Configure(EntityTypeBuilder<TrExperienceType> builder)
        {
            builder.Property(x => x.IdExperience)
                .HasMaxLength(36)
                .IsRequired();

            builder.Property(x => x.ExperienceType)
                .HasMaxLength(maxLength: 20)
                .HasConversion(valueToDb =>
                valueToDb.ToString(),
                valueFromDb => (ExperienceType)Enum.Parse(typeof(ExperienceType), valueFromDb))
                .IsRequired();

            builder.HasOne(x => x.Experience)
                .WithMany(x => x.TrExperienceTypes)
                .IsRequired()
                .HasForeignKey(fk => fk.IdExperience)
                .HasConstraintName("FK_TrExperienceType_TrExperience")
                .OnDelete(DeleteBehavior.Cascade);

            base.Configure(builder);
        }
    }
}
