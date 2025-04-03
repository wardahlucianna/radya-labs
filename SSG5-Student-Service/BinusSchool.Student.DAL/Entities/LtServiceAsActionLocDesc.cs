using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities
{
    public class LtServiceAsActionLocDesc : AuditEntity, IStudentEntity
    {
        public string SALocDes { get; set; }

        public virtual ICollection<TrServiceAsActionForm> ServiceAsActionForms { get; set; }

    }

    internal class MsExperienceLocationConfiguration : AuditEntityConfiguration<LtServiceAsActionLocDesc>
    {
        public override void Configure(EntityTypeBuilder<LtServiceAsActionLocDesc> builder)
        {
            builder.Property(x => x.SALocDes).IsRequired().HasMaxLength(50);

            base.Configure(builder);
        }
    }
}
