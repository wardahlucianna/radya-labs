using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.UserDb.Abstractions;
using BinusSchool.Persistence.UserDb.Entities.Scheduling;
using BinusSchool.Persistence.UserDb.Entities.Student;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.UserDb.Entities.School
{
    public class MsGrade : CodeEntity, IUserEntity
    {
        public string IdLevel { get; set; }
        public int OrderNumber { get; set; }
        public virtual MsLevel MsLevel { get; set; }
        public virtual ICollection<MsGradePathway> MsGradePathways { get; set; }
        public virtual ICollection<MsHomeroom> Homerooms { get; set; }
        public virtual ICollection<MsStudentGrade> StudentGrades { get; set; }
        public virtual ICollection<MsSubject> Subjects { get; set; }
        public virtual ICollection<MsLesson> Lessons { get; set; }
        public virtual ICollection<MsPeriod> Periods { get; set; }
        public virtual ICollection<TrMessageForGrade> MessageForGrades { get; set; }
    }

    internal class MsGradeConfiguration : CodeEntityConfiguration<MsGrade>
    {
        public override void Configure(EntityTypeBuilder<MsGrade> builder)
        {
            builder.HasOne(x => x.MsLevel)
                .WithMany(x => x.MsGrades)
                .HasForeignKey(fk => fk.IdLevel)
                .HasConstraintName("FK_MsLevel_MsGrade")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();


            base.Configure(builder);
        }
    }
}
