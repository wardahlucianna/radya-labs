using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities.School
{
    public class MsClassroom : CodeEntity, IStudentEntity
    {
        public string IdSchool { get; set; }

        public virtual MsSchool School { get; set; }
        public virtual ICollection<MsGradePathwayClassroom> GradePathwayClassrooms { get; set; }
    }

    internal class MsClassroomConfiguration : CodeEntityConfiguration<MsClassroom>
    {
        public override void Configure(EntityTypeBuilder<MsClassroom> builder)
        {
            builder.HasOne(x => x.School)
                .WithMany(x => x.Classrooms)
                .HasForeignKey(fk => fk.IdSchool)
                .HasConstraintName("FK_MsClassRooms_MsSchool")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
