using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities
{
    public class MsServiceAsActionType : AuditEntity, IStudentEntity
    {
        public string TypeDesc { get; set; }

        public virtual ICollection<TrServiceAsActionMappingType> ServiceAsActionMappingTypes { get; set; }
    }

    internal class MsExperienceTypeConfiguration : AuditEntityConfiguration<MsServiceAsActionType>
    {
        public override void Configure(EntityTypeBuilder<MsServiceAsActionType> builder)
        {
            builder.Property(x => x.TypeDesc).IsRequired().HasMaxLength(50);

            base.Configure(builder);
        }
    }
}
