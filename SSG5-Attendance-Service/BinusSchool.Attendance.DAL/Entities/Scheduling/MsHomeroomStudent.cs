using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities;
using BinusSchool.Persistence.AttendanceDb.Entities.Student;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Collections.Generic;

namespace BinusSchool.Persistence.AttendanceDb.Entities.Scheduling
{
    public class MsHomeroomStudent : AuditEntity, IAttendanceEntity
    {
        public string IdHomeroom { get; set; }
        public string IdStudent { get; set; }

        public virtual MsStudent Student { get; set; }
        public virtual MsHomeroom Homeroom { get; set; }
        public virtual ICollection<MsHomeroomStudentEnrollment> HomeroomStudentEnrollments { get; set; }
        public virtual ICollection<TrAttendanceEntryV2> AttendanceEntryV2s { get; set; }
        public virtual ICollection<TrHomeroomStudentEnrollment> TrHomeroomStudentEnrollments { get; set; }
        public virtual ICollection<HTrMoveStudentHomeroom> HTrMoveStudentHomerooms { get; set; }
    }

    internal class MsHomeroomStudentConfiguration : AuditEntityConfiguration<MsHomeroomStudent>
    {
        public override void Configure(EntityTypeBuilder<MsHomeroomStudent> builder)
        {
            builder.Property(x => x.IdStudent)
                .HasMaxLength(36)
                .IsRequired();

            builder.HasOne(x => x.Student)
                .WithMany(x => x.HomeroomStudents)
                .HasForeignKey(fk => fk.IdStudent)
                .HasConstraintName("FK_MsHomeroomStudent_MsStudent")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            builder.HasOne(x => x.Homeroom)
                .WithMany(x => x.HomeroomStudents)
                .HasForeignKey(fk => fk.IdHomeroom)
                .HasConstraintName("FK_MsHomeroomStudent_MsHomeroom")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
