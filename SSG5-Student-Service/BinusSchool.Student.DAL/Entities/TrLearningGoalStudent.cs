using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities.School;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities
{
    public class TrLearningGoalStudent : AuditEntity, IStudentEntity
    {        
        public string Name { get; set; }
        public string IdStudent { get; set; }
        public string IdProfile { get; set; }
        public bool IsOngoing { get; set; }
        public string IdAcademicYear { get; set; }
        public int Semester { get; set; }
        public virtual MsStudent Student { get; set; }
        public virtual MsLearnerProfile LearnerProfile { get; set; }
        public virtual MsAcademicYear AcademicYear { get; set; }
    }

    internal class TrLearningGoalStudentProfilekConfiguration : AuditEntityConfiguration<TrLearningGoalStudent>
    {
        public override void Configure(EntityTypeBuilder<TrLearningGoalStudent> builder)
        {
            builder.Property(x => x.Name)
                .HasMaxLength(200)
                .IsRequired(true);

            builder.Property(x => x.IsOngoing)
                .IsRequired(true);

            builder.HasOne(x => x.Student)
                .WithMany(x => x.LearningGoalStudents)
                .HasForeignKey(fk => fk.IdStudent)
                .HasConstraintName("FK_TrLearningGoalStudent_Student")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder.HasOne(x => x.LearnerProfile)
                .WithMany(x => x.LearningGoalStudent)
                .HasForeignKey(fk => fk.IdProfile)
                .HasConstraintName("FK_TrLearningGoalStudent_MsLearnerProfile")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();
            builder.HasOne(x => x.AcademicYear)
                .WithMany(x => x.LearningGoalStudents)
                .HasForeignKey(fk => fk.IdAcademicYear)
                .HasConstraintName("FK_TrLearningGoalStudent_MsAcademicYear")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            base.Configure(builder); 

        }
    }

}
