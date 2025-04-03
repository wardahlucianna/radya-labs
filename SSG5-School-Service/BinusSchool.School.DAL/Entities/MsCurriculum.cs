using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchoolDb.Entities
{
    public class MsCurriculum : CodeEntity, ISchoolEntity
    {
        public string IdSchool { get; set; }

        public virtual MsSchool School { get; set; }
        public virtual ICollection<MsSubject> Subjects { get; set; }
    }

    internal class MsCurriculumConfiguration : CodeEntityConfiguration<MsCurriculum>
    {
        public override void Configure(EntityTypeBuilder<MsCurriculum> builder)
        {
            builder.HasOne(x => x.School)
                .WithMany(x => x.Curriculums)
                .HasForeignKey(fk => fk.IdSchool)
                .HasConstraintName("FK_MsCurriculum_MsSchool")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();


            base.Configure(builder);
        }
    }
}
