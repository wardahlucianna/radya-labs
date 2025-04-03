using System;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchoolDb.Entities
{
    public class MsDurationSetting : AuditEntity, ISchoolEntity
    {
        public string Duration { get; set; }
        public TypeDuration Type { get; set; }
    }

    internal class MsDurationSettingConfiguration : AuditEntityConfiguration<MsDurationSetting>
    {
        public override void Configure(EntityTypeBuilder<MsDurationSetting> builder)
        {
            builder.Property(e => e.Type).HasMaxLength(maxLength: 15)
               .HasConversion(valueToDb =>
               valueToDb.ToString(),
               valueFromDb => (TypeDuration)Enum.Parse(typeof(TypeDuration), valueFromDb))
               .IsRequired();

            builder.Property(x => x.Duration)
             .HasMaxLength(1054)
             .IsRequired();

            base.Configure(builder);
        }
    }
}
