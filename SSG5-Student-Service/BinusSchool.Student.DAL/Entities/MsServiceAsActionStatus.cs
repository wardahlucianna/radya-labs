using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities
{
    public class MsServiceAsActionStatus : AuditEntity, IStudentEntity
    {
        public string StatusDesc { get; set; }
        public bool? IsOverall { get; set; }
        public bool? IsDetail { get; set; }

        public virtual ICollection<TrServiceAsActionHeader> ServiceAsActionHeaders { get; set; }
        public virtual ICollection<TrServiceAsActionForm> ServiceAsActionForms { get; set; }
    }

    internal class MsExperienceStatusConfiguration : AuditEntityConfiguration<MsServiceAsActionStatus>
    {
        public override void Configure(EntityTypeBuilder<MsServiceAsActionStatus> builder)
        {
            builder.Property(x => x.StatusDesc).IsRequired().HasMaxLength(25);

            base.Configure(builder);
        }
    }
}
