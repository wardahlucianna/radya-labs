using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchedulingDb.Entities.Student
{
    public class MsStudent : UserKindStudentParentEntity, ISchedulingEntity
    {
        public string IdReligion { get; set; }
        public string IdSchool { get; set; }
        public string BinusianEmailAddress { get; set; }
        public int IdStudentStatus { get; set; }

        public virtual LtReligion Religion { get; set; }
        public virtual LtStudentStatus StudentStatus { get; set; }
        public virtual MsSchool School { get; set; }
        public virtual MsSiblingGroup SiblingGroup { get; set; }

        public virtual ICollection<TrGeneratedScheduleStudent> GeneratedScheduleStudents { get; set; }
        public virtual ICollection<MsStudentGrade> StudentGrades { get; set; }
        public virtual ICollection<MsHomeroomStudent> HomeroomStudents { get; set; }
        public virtual ICollection<MsExtracurricularParticipant> ExtracurricularParticipants { get; set; }
        public virtual ICollection<HMsExtracurricularParticipant> HMsExtracurricularParticipants { get; set; }
        public virtual ICollection<TrExtracurricularScoreEntry> ExtracurricularScoreEntries { get; set; }
        public virtual ICollection<TrExtracurricularAttendanceEntry> ExtracurricularAttendanceEntries { get; set; }
        public virtual ICollection<TrEventIntendedForPersonalStudent> TrEventIntendedForPersonalStudents { get; set; }
        public virtual ICollection<HTrEventIntendedForPersonalStudent> HistoryEventIntendedForPersonalStudents { get; set; }
        public virtual ICollection<MsStudentParent> StudentParents { get; set; }
        public virtual ICollection<TrPersonalInvitation> PersonalInvitations { get; set; }
        public virtual ICollection<TrStudentStatus> TrStudentStatuss { get; set; }
        public virtual ICollection<TrGeneratedScheduleLesson> GeneratedScheduleLessons { get; set; }
        public virtual ICollection<HTrStudentProgramme> HTrStudentProgrammes { get; set; }
        public virtual ICollection<TrStudentProgramme> StudentProgrammes { get; set; }
    }

    internal class MsStudentConfiguration : UserKindStudentParentEntityConfiguration<MsStudent>
    {
        public override void Configure(EntityTypeBuilder<MsStudent> builder)
        {
            builder.Property(x => x.IdReligion)
               .HasMaxLength(36);

            builder.Property(x => x.IdSchool)
               .HasMaxLength(36);

            builder.Property(x => x.BinusianEmailAddress)
                .HasColumnType("VARCHAR(200)")
                .HasMaxLength(200);

            builder.HasOne(x => x.Religion)
                .WithMany(y => y.Student)
                .HasForeignKey(fk => fk.IdReligion)
                .HasConstraintName("FK_MsStudent_LtReligion")
                .OnDelete(DeleteBehavior.SetNull);
            base.Configure(builder);

            builder.HasOne(x => x.School)
                .WithMany(y => y.Students)
                .HasForeignKey(fk => fk.IdSchool)
                .HasConstraintName("FK_MsStudent_MsSchool")
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.StudentStatus)
               .WithMany(y => y.Students)
               .HasForeignKey(fk => fk.IdStudentStatus)
               .HasConstraintName("FK_MsStudent_LtStudentStatus")
               .OnDelete(DeleteBehavior.Restrict);
            base.Configure(builder);
        }
    }
}
