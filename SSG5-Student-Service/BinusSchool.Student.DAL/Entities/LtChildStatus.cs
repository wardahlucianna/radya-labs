using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities
{
    public class LtChildStatus : AuditEntity, IStudentEntity
    {
        public string ChildStatusName { get; set; }

        public virtual ICollection<MsStudent> Student { get; set; }   
        
    }
    internal class LtChildStatusConfiguration : AuditEntityConfiguration<LtChildStatus>
    {
        public override void Configure(EntityTypeBuilder<LtChildStatus> builder)
        {
            builder.Property(x => x.ChildStatusName)
                .HasMaxLength(30);

            base.Configure(builder);
        }

    }
}
