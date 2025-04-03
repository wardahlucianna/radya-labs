using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.TeachingDb.Abstractions;
using BinusSchool.Persistence.TeachingDb.Entities.Scheduling;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.TeachingDb.Entities.School
{
    public class MsGrade : CodeEntity, ITeachingEntity
    {
        public string IdLevel { get; set; }
        public int OrderNumber { get; set; }
        public virtual MsLevel Level { get; set; }
        public virtual ICollection<MsPeriod> Periods { get; set; }
        public virtual ICollection<MsSubject> Subjects { get; set; }
        public virtual ICollection<MsGradePathway> GradePathways { get; set; }
        public virtual ICollection<MsHomeroom> Homerooms { get; set; }
        public virtual ICollection<MsLesson> Lessons { get; set; }
    }

    internal class MsGradeConfiguration : CodeEntityConfiguration<MsGrade>
    {
        public override void Configure(EntityTypeBuilder<MsGrade> builder)
        {
            builder.HasOne(x => x.Level)
                .WithMany(x => x.Grades)
                .HasForeignKey(fk => fk.IdLevel)
                .HasConstraintName("FK_MsGrade_MsLevel")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();


            base.Configure(builder);
        }
    }
}
