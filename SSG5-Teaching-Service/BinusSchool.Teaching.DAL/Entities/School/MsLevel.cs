using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.TeachingDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.TeachingDb.Entities.School
{
    public class MsLevel : CodeEntity, ITeachingEntity
    {
        public string IdAcademicYear { get; set; }
        public int OrderNumber { get; set; }
        public virtual MsAcademicYear AcademicYear { get; set; }
        public virtual ICollection<MsDepartmentLevel> DepartmentLevels { get; set; }
        public virtual ICollection<MsGrade> Grades { get; set; }
        public virtual ICollection<MsLevelApproval> LevelApprovals { get; set; }
        public virtual ICollection<MsTeacherPositionAlias> TeacherPositionAliases { get; set; }
    }

    internal class MsLevelConfiguration : CodeEntityConfiguration<MsLevel>
    {
        public override void Configure(EntityTypeBuilder<MsLevel> builder)
        {
            builder.HasOne(x => x.AcademicYear)
                .WithMany(x => x.Levels)
                .HasForeignKey(fk => fk.IdAcademicYear)
                .HasConstraintName("FK_MsLevel_MsAcademicYear")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();


            base.Configure(builder);
        }
    }
}
