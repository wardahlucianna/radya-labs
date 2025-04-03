using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities.Schedulling;
using BinusSchool.Persistence.StudentDb.Entities.School;
using BinusSchool.Student.DAL.Entities;
using BinusSchool.Student.DAL.Entities.School;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities.Scheduling
{
    public class MsSubject : CodeEntity, IStudentEntity
    {
        public string IdGrade { get; set; }
        public string IdDepartment { get; set; }
        public string SubjectID { get; set; }
        public int MaxSession { get; set; }
        public string IdSubjectGroup { get; set; }
        public virtual MsGrade Grade { get; set; }
        public virtual MsDepartment Departement { get; set; }
        public virtual ICollection<MsLesson> Lessons { get; set; }
        public virtual ICollection<MsHomeroomStudentEnrollment> HomeroomStudentEnrollments { get; set; }
        public virtual ICollection<MsSubjectSelectionRuleEnrollment> SubjectSelectionRuleEnrollments { get; set; }
        public virtual ICollection<MsMappingCurriculumSubjectGroupDtl> MappingCurriculumSubjectGroupDtls { get; set; }
        public virtual ICollection<MsSubjectMappingSubjectLevel> SubjectMappingSubjectLevels { get; set; }
    }

    internal class MsSubjectConfiguration : CodeEntityConfiguration<MsSubject>
    {
        public override void Configure(EntityTypeBuilder<MsSubject> builder)
        {
            builder.Property(x => x.SubjectID)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(x => x.IdSubjectGroup)
                .HasMaxLength(36);

            builder.HasOne(x => x.Grade)
              .WithMany(x => x.Subjects)
              .HasForeignKey(fk => fk.IdGrade)
              .HasConstraintName("FK_MsSubject_MsGrade")
              .OnDelete(DeleteBehavior.NoAction)
              .IsRequired();

            builder.HasOne(x => x.Departement)
              .WithMany(x => x.Subjects)
              .HasForeignKey(fk => fk.IdDepartment)
              .HasConstraintName("FK_MsSubject_MsDepartement")
              .OnDelete(DeleteBehavior.Cascade)
              .IsRequired();

            base.Configure(builder);
        }
    }
}
