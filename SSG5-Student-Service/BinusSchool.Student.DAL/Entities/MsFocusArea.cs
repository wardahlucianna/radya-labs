using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities
{
    public class MsFocusArea : AuditEntity, IStudentEntity
    {
        public string Description { get; set; }
        public virtual ICollection<TrEntryMeritStudent> EntryMeritStudents { get; set; }

    }
    internal class MsFocusAreaConfiguration : AuditEntityConfiguration<MsFocusArea>
    {
        public override void Configure(EntityTypeBuilder<MsFocusArea> builder)
        {
            builder.Property(x => x.Description)
                .HasMaxLength(1054);

            base.Configure(builder);
        }
    }
}
