using BinusSchool.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Collections.Generic;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.Persistence.SchoolDb.Entities.Student;

namespace BinusSchool.Persistence.SchoolDb.Entities.Scheduling
{
    public class MsHomeroomStudent : AuditEntity, ISchoolEntity
    {
        public string IdHomeroom { get; set; }
        public string IdStudent { get; set; }

        public virtual MsStudent Student { get; set; }
        public virtual MsHomeroom Homeroom { get; set; }
        public virtual ICollection<TrPublishSurveyMapping> PublishSurveyMappings { get; set; }
        public virtual ICollection<TrSurvey> Surveys { get; set; }
        public virtual ICollection<MsHomeroomStudentEnrollment> HomeroomStudentEnrollments { get; set; }
    }

    internal class MsHomeroomStudentConfiguration : AuditEntityConfiguration<MsHomeroomStudent>
    {
        public override void Configure(EntityTypeBuilder<MsHomeroomStudent> builder)
        {
            builder.Property(x => x.IdStudent)
                .HasMaxLength(36)
                .IsRequired();

            builder.HasOne(x => x.Student)
                .WithMany(x => x.HomeroomStudents)
                .HasForeignKey(fk => fk.IdStudent)
                .HasConstraintName("FK_MsHomeroomStudent_MsStudent")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            builder.HasOne(x => x.Homeroom)
                .WithMany(x => x.HomeroomStudents)
                .HasForeignKey(fk => fk.IdHomeroom)
                .HasConstraintName("FK_MsHomeroomStudent_MsHomeroom")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
