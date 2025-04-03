using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.TeachingDb.Abstractions;
using BinusSchool.Persistence.TeachingDb.Entities.User;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.TeachingDb.Entities.School
{
    public class MsSchool : AuditEntity, ITeachingEntity
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Address { get; set; }
        public virtual ICollection<MsAcademicYear> AcademicYears { get; set; }
        public virtual ICollection<MsTeacherPosition> TeacherPositions { get; set; }
        public virtual ICollection<MsDivision> Divisions { get; set; }
        public virtual ICollection<MsBuilding> Buildings { get; set; }
        public virtual ICollection<MsUserSchool> UserSchools { get; set; }
        public virtual ICollection<LtRole> Roles { get; set; }
        public virtual ICollection<MsSubjectLevel> SubjectLevels { get; set; }
        public virtual ICollection<MsLessonApproval> LessonApprovals { get; set; }
        public virtual ICollection<MsClassroom> Classrooms { get; set; }
    }

    internal class MsSchoolConfiguration : AuditEntityConfiguration<MsSchool>
    {
        public override void Configure(EntityTypeBuilder<MsSchool> builder)
        {
            builder.Property(x => x.Name)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(x => x.Description)
                .HasMaxLength(128)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
