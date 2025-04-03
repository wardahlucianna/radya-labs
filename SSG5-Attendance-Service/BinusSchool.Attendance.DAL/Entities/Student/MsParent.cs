using System.Collections.Generic;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.AttendanceDb.Entities.Student
{
    public class MsParent : UserKindStudentParentEntity, IAttendanceEntity
    {
        public string PersonalEmailAddress { get; set; }
        public string WorkEmailAddress { get; set; }
        public virtual ICollection<MsStudentParent> StudentParents { get; set; }
    }

    internal class MsParentConfiguration : UserKindStudentParentEntityConfiguration<MsParent>
    {
        public override void Configure(EntityTypeBuilder<MsParent> builder)
        {
             builder.Property(x => x.PersonalEmailAddress) 
                .HasColumnType("VARCHAR(50)")     
                .HasMaxLength(50); 

            builder.Property(x => x.WorkEmailAddress) 
                .HasColumnType("VARCHAR(50)")     
                .HasMaxLength(50);
                                       
            base.Configure(builder);
        }
    }
}
