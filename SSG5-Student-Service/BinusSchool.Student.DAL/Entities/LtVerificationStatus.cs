using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities
{
    public class LtVerificationStatus : AuditEntity, IStudentEntity
    {
        public string VerificationStatusName { get; set; }
        public virtual ICollection<TrStudentDocument> StudentDocument { get; set; }
    }

    internal class LtVerificationStatusConfiguration : AuditEntityConfiguration<LtVerificationStatus>
    {
        public override void Configure(EntityTypeBuilder<LtVerificationStatus> builder)
        {
            builder.Property(x => x.VerificationStatusName)             
                .HasMaxLength(36);

             base.Configure(builder);
        }
    }
}
