using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.AttendanceDb.Entities.School
{
    public class MsClassroom : CodeEntity, IAttendanceEntity
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
                .HasConstraintName("FK_MsClassroom_MsSchool")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
