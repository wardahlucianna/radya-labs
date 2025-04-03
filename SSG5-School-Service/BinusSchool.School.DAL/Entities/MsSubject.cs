using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.Persistence.SchoolDb.Entities.Scheduling;
using BinusSchool.Persistence.SchoolDb.Entities.Teaching;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchoolDb.Entities
{
    public class MsSubject : CodeEntity, ISchoolEntity
    {
        public string IdGrade { get; set; }
        public string IdCurriculum { get; set; }
        public string IdDepartment { get; set; }
        public string IdSubjectType { get; set; }
        public string IdSubjectGroup { get; set; }
        public string SubjectID { get; set; }
        public int MaxSession { get; set; }
        public bool? IsNeedLessonPlan { get; set; }

        public virtual MsGrade Grade { get; set; }
        public virtual MsCurriculum Curriculum { get; set; }
        public virtual MsDepartment Department { get; set; }
        public virtual MsSubjectType SubjectType { get; set; }
        public virtual MsSubjectGroup SubjectGroup { get; set; }
        public virtual ICollection<MsSubjectCombination> SubjectCombinations { get; set; }
        //public virtual ICollection<MsSubjectLevel> SubjectLevels { get; set; }
        public virtual ICollection<MsSubjectPathway> SubjectPathways { get; set; }
        public virtual ICollection<MsSubjectSession> SubjectSessions { get; set; }
        public virtual ICollection<MsSubjectMappingSubjectLevel> SubjectMappingSubjectLevels { get; set; }
        public virtual ICollection<MsTextbookSubjectGroupDetail> TextbookSubjectGroupDetails { get; set; }
        public virtual ICollection<TrTextbook> Textbooks { get; set; }
        public virtual ICollection<MsLesson> Lessons { get; set; }
        public virtual ICollection<MsHomeroomStudentEnrollment> HomeroomStudentEnrollments { get; set; }
    }

    internal class MsSubjectConfiguration : CodeEntityConfiguration<MsSubject>
    {
        public override void Configure(EntityTypeBuilder<MsSubject> builder)
        {
            builder.Property(x => x.SubjectID)
                .HasMaxLength(50)
                .IsRequired();
                
            builder.HasOne(x => x.Grade)
                .WithMany(x => x.Subjects)
                .HasForeignKey(fk => fk.IdGrade)
                .HasConstraintName("FK_MsSubject_MsGrade")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();


            builder.HasOne(x => x.Curriculum)
                .WithMany(x => x.Subjects)
                .HasForeignKey(fk => fk.IdCurriculum)
                .HasConstraintName("FK_MsSubject_MsCurriculum")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder.HasOne(x => x.Department)
                .WithMany(x => x.Subjects)
                .HasForeignKey(fk => fk.IdDepartment)
                .HasConstraintName("FK_MsSubject_MsDepartment")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder.HasOne(x => x.SubjectType)
                .WithMany(x => x.Subjects)
                .HasForeignKey(fk => fk.IdSubjectType)
                .HasConstraintName("FK_MsSubject_MsSubjectType")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder.HasOne(x => x.SubjectGroup)
                .WithMany(x => x.Subjects)
                .HasForeignKey(fk => fk.IdSubjectGroup)
                .HasConstraintName("FK_MsSubject_MsSubjectGroup")
                .OnDelete(DeleteBehavior.NoAction);

            base.Configure(builder);
        }
    }
}
