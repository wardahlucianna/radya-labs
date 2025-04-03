using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.UserDb.Abstractions;
using BinusSchool.Persistence.UserDb.Entities.Scheduling;
using BinusSchool.Persistence.UserDb.Entities.School;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.UserDb.Entities.Student
{
    public class MsStudent : UserKindStudentParentEntity, IUserEntity
    {
        public string IdSchool { get; set; }
        public virtual ICollection<MsStudentGrade> StudentGrades { get; set; }
        public virtual ICollection<MsStudentParent> StudentParents { get; set; }
        public virtual MsSchool School { get; set; }
        public virtual ICollection<MsStudentBlocking> StudentBlockings { get; set; }
        public virtual ICollection<HMsStudentBlocking> HistoryStudentBlockings { get; set; }
        public virtual ICollection<MsHomeroomStudent> HomeroomStudents { get; set; }
    }

    internal class MsStudentConfiguration : UserKindStudentParentEntityConfiguration<MsStudent>
    {
        public override void Configure(EntityTypeBuilder<MsStudent> builder)
        {
            builder.HasOne(x => x.School)
                .WithMany(x => x.Students)
                .HasForeignKey(fk => fk.IdSchool)
                .HasConstraintName("FK_MsStudent_MsSchool")
                .OnDelete(DeleteBehavior.Restrict);
            base.Configure(builder);
        }
    }
}
