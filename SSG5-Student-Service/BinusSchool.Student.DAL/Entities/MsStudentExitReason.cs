using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities
{
    public class MsStudentExitReason : AuditEntity, IStudentEntity
    {
        public string Description { get; set; }
        public virtual ICollection<TrStudentExitReason> TrStudentExitReasons { get; set; }

    }

    internal class MsStudentExitReasonConfiguration : AuditEntityConfiguration<MsStudentExitReason>
    {
        public override void Configure(EntityTypeBuilder<MsStudentExitReason> builder)
        {

            builder.Property(x => x.Description)
               .HasMaxLength(1054)
               .IsRequired();

            base.Configure(builder);
        }

    }
}
