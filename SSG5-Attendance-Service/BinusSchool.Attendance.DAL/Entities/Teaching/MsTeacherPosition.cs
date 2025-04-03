using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities.Scheduling;
using BinusSchool.Persistence.AttendanceDb.Entities.School;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BinusSchool.Persistence.AttendanceDb.Entities.Teaching;
using BinusSchool.Persistence.AttendanceDb.Entities.User;

namespace BinusSchool.Persistence.AttendanceDb.Entities.Teaching
{
    public class MsTeacherPosition : CodeEntity, IAttendanceEntity
    {
        public string IdSchool { get; set; }
        public string IdPosition { get; set; }

        public virtual MsSchool School { get; set; }
        public virtual LtPosition LtPosition { get; set; }
        public virtual ICollection<MsAbsentMappingAttendance> AbsentMappingAttendances { get; set; }
        public virtual ICollection<MsHomeroomTeacher> HomeroomTeachers { get; set; }
        public virtual ICollection<MsNonTeachingLoad> NonTeachingLoads { get; set; }
        public virtual ICollection<TrRolePosition> RolePositions { get; set; }
    }

    internal class MsTeacherPositionConfiguration : CodeEntityConfiguration<MsTeacherPosition>
    {
        public override void Configure(EntityTypeBuilder<MsTeacherPosition> builder)
        {
            builder.HasOne(x => x.LtPosition)
                .WithMany(x => x.TeacherPositions)
                .HasForeignKey(fk => fk.IdPosition)
                .HasConstraintName("FK_MsTeacherPosition_LtPosition")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            builder.HasOne(x => x.School)
                .WithMany(x => x.TeacherPositions)
                .HasForeignKey(fk => fk.IdSchool)
                .HasConstraintName("FK_MsTeacherPosition_MsSchool")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
