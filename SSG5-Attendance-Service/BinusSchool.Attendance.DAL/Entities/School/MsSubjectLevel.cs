using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities.Scheduling;
using BinusSchool.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.AttendanceDb.Entities.School
{
    public class MsSubjectLevel : CodeEntity, IAttendanceEntity
    {
        public string IdSchool { get; set; }
        public virtual MsSchool School { get; set; }

        public virtual ICollection<TrHomeroomStudentEnrollment> TrHomeroomStudentEnrollmentsNews { get; set; }
        public virtual ICollection<TrHomeroomStudentEnrollment> TrHomeroomStudentEnrollmentsOlds { get; set; }
    }

    internal class MsSubjectLevelConfiguration : CodeEntityConfiguration<MsSubjectLevel>
    {
        public override void Configure(EntityTypeBuilder<MsSubjectLevel> builder)
        {
            builder.HasOne(x => x.School)
                .WithMany(x => x.SubjectLevels)
                .HasForeignKey(fk => fk.IdSchool)
                .HasConstraintName("FK_MsSubjectLevel_MsSchool")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
