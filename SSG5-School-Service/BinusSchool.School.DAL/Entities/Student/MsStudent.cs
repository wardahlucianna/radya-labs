using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.Persistence.SchoolDb.Entities.Scheduling;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchoolDb.Entities.Student
{
    public class MsStudent : UserKindStudentParentEntity, ISchoolEntity
    {
        public int IdStudentStatus { get; set; }
        public string IdBinusian { get; set; }
        public string IdReligion { get; set; }
        public virtual LtReligion Religion { get; set; }
        public virtual ICollection<MsHomeroomStudent> HomeroomStudents { get; set; }
    }

    internal class MsStudentConfiguration : UserKindStudentParentEntityConfiguration<MsStudent>
    {
        public override void Configure(EntityTypeBuilder<MsStudent> builder)
        {
            builder.Property(x => x.IdBinusian)
               .HasMaxLength(36)
               .IsRequired();

            builder.HasOne(x => x.Religion)
               .WithMany(y => y.Students)
               .HasForeignKey(fk => fk.IdReligion)
               .HasConstraintName("FK_MsStudent_LtReligion")
               .OnDelete(DeleteBehavior.SetNull);

            builder.Property(x => x.IdReligion)
                .HasMaxLength(36);

            base.Configure(builder);
        }
    }
}
