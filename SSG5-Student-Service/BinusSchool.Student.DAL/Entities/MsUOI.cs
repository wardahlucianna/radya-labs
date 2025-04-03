using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities
{
    public class MsUOI : AuditEntity, IStudentEntity
    {        
        public string Name { get; set; }
        public virtual ICollection<TrCourseworkAnecdotalStudent> CourseworkAnecdotalStudents { get; set; }
    }

    internal class MsUOIConfiguration : AuditEntityConfiguration<MsUOI>
    {
        public override void Configure(EntityTypeBuilder<MsUOI> builder)
        {        
            builder.Property(x => x.Name)             
                .HasMaxLength(100);

             base.Configure(builder); 

        }
    }

}
