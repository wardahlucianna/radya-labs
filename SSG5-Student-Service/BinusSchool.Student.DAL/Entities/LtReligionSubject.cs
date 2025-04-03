using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities
{
    public class LtReligionSubject : AuditEntity, IStudentEntity
    {
        public string ReligionSubjectName { get; set; }

        public virtual ICollection<MsStudent> Student { get; set; }
    }
     internal class LtReligionSubjectConfiguration : AuditEntityConfiguration<LtReligionSubject>
    {
        public override void Configure(EntityTypeBuilder<LtReligionSubject> builder)
        {
            builder.Property(x => x.ReligionSubjectName)
                .HasMaxLength(36);

            base.Configure(builder);
        }

    }
}
