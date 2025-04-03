using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities
{
    public class MsPreviousSchoolOld : AuditEntity, IStudentEntity
    {
        public string IdOldSchool { get; set; }
        public string IdSchool { get; set; }
        public string SchoolName { get; set; }
       
        public virtual ICollection<MsStudentPrevSchoolInfo> StudentPrevSchoolInfo { get; set; }
    }

    internal class MsPreviousSchoolOldConfiguration : AuditEntityConfiguration<MsPreviousSchoolOld>
    {
        public override void Configure(EntityTypeBuilder<MsPreviousSchoolOld> builder)
        {
            builder.Property(x => x.IdSchool)           
                .HasMaxLength(36);     

            builder.Property(x => x.SchoolName)
                .HasMaxLength(100);  

            builder.Property(x => x.IdOldSchool)           
                .HasMaxLength(20);     
         
       
            base.Configure(builder);

        }
    }
}
