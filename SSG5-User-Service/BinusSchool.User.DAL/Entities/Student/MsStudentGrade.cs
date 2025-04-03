using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.UserDb.Abstractions;
using BinusSchool.Persistence.UserDb.Entities.School;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.UserDb.Entities.Student
{
    public class MsStudentGrade : AuditEntity, IUserEntity
    {
        public string IdStudent { get; set; }
        public string IdGrade { get; set; }

        public virtual MsStudent Student { get; set; }
        public virtual MsGrade Grade {get;set;}
    }

    internal class MsStudentGradeConfiguration : AuditEntityConfiguration<MsStudentGrade>
    {
        public override void Configure(EntityTypeBuilder<MsStudentGrade> builder)
        {
            builder.HasOne(x => x.Student)
                .WithMany(x => x.StudentGrades)
                .HasForeignKey(fk => fk.IdStudent)
                .HasConstraintName("FK_MsStudentGrade_MsStudent")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            builder.HasOne(x => x.Grade)
                .WithMany(x => x.StudentGrades)
                .HasForeignKey(fk => fk.IdGrade)
                .HasConstraintName("FK_MsStudentGrade_MsGrade")
                .OnDelete(DeleteBehavior.Cascade);

            base.Configure(builder);
        }
    }
}
