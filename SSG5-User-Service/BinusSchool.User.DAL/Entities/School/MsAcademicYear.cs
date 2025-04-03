using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.TeachingDb.Entities;
using BinusSchool.Persistence.UserDb.Abstractions;
using BinusSchool.Persistence.UserDb.Entities.Scheduling;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.UserDb.Entities.School
{
    public class MsAcademicYear : CodeEntity, IUserEntity
    {
        public string IdSchool { get; set; }
        public int OrderNumber { get; set; }
        public virtual MsSchool MsSchool { get; set; }
        public virtual ICollection<MsLevel> MsLevels { get; set; }
        public virtual ICollection<MsHomeroom> Homerooms { get; set; }
        public virtual ICollection<MsPathway> Pathways { get; set; }
        public virtual ICollection<MsNonTeachingLoad> NonTeachingLoads { get; set; }
        public virtual ICollection<MsDepartment> Departments { get; set; }
        public virtual ICollection<MsLesson> Lessons { get; set; }
    }

    internal class MsAcademicYearConfiguration : CodeEntityConfiguration<MsAcademicYear>
    {
        public override void Configure(EntityTypeBuilder<MsAcademicYear> builder)
        {
            builder.HasOne(x => x.MsSchool)
                .WithMany(x => x.AcademicYears)
                .HasForeignKey(fk => fk.IdSchool)
                .HasConstraintName("FK_MsAcademicYear_MsSchool")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
