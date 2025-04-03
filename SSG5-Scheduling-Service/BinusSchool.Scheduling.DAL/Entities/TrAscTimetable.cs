using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchedulingDb.Entities
{
    public class TrAscTimetable : AuditEntity, ISchedulingEntity
    {
        public string IdAcademicYear { get; set; }
        public string Name { get; set; }
        public string IdSessionSet { get; set; }
        public string IdSchool { get; set; }
        public bool IsAutomaticGenerateClassId { get; set; }
        public string FormatClassName { get; set; }
        public string ExampleFormatClassId { get; set; }
        public string GradeCodeForGenerateClassId { get; set; }
        public string IdGradePathwayForParticipant { get; set; }
        public string XmlFileName { get; set; }

        public virtual ICollection<TrAscTimetableLesson> AscTimetableLessons { get; set; }
        public virtual ICollection<TrAscTimetableHomeroom> AscTimetableHomerooms { get; set; }
        public virtual ICollection<TrAscTimetableSchedule> AscTimetableSchedules { get; set; }
        public virtual ICollection<TrAscTimetableDayStructure> AscTimetableDayStructures { get; set; }
        public virtual ICollection<TrAscTimetableEnrollment> AscTimetableEnrollments { get; set; }
        public virtual ICollection<TrGeneratedSchedule> GeneratedSchedules { get; set; }
        public virtual MsSchool School { get; set; }
        public virtual MsAcademicYear AcademicYear { get; set; }
        public virtual MsSessionSet SessionSet { get; set; }
    }

    internal class TrAscTimetableConfiguration : AuditEntityConfiguration<TrAscTimetable>
    {
        public override void Configure(EntityTypeBuilder<TrAscTimetable> builder)
        {
            builder.Property(x => x.Name)
               .HasMaxLength(36);

            builder.HasOne(x => x.SessionSet)
            .WithMany(x => x.AscTimetables)
            .HasForeignKey(fk => fk.IdSessionSet)
            .HasConstraintName("FK_TrAsctimetable_MsSessionSet")
            .OnDelete(DeleteBehavior.NoAction)
              .IsRequired();

            builder.HasOne(x => x.AcademicYear)
              .WithMany(x => x.AscTimetables)
              .HasForeignKey(fk => fk.IdAcademicYear)
              .HasConstraintName("FK_TrAsctimetable_MsAcademicYear")
              .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder.HasOne(x => x.School)
              .WithMany(x => x.AscTimetables)
              .HasForeignKey(fk => fk.IdSchool)
              .HasConstraintName("FK_MsHomeroom_MsSchool")
              .OnDelete(DeleteBehavior.NoAction)
            .IsRequired();



            builder.Property(x => x.FormatClassName)
               .HasMaxLength(450);

            builder.Property(x => x.XmlFileName)
               .HasMaxLength(450);


            base.Configure(builder);
        }
    }
}
