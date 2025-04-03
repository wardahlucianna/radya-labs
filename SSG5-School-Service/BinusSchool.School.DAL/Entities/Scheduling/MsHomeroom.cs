using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BinusSchool.Persistence.SchoolDb.Abstractions;

namespace BinusSchool.Persistence.SchoolDb.Entities.Scheduling
{
    public class MsHomeroom : AuditEntity, ISchoolEntity
    {
        public string IdAcademicYear { get; set; }
        public int Semester { get; set; }
        public string IdGrade { get; set; }
        public string IdGradePathwayClassRoom { get; set; }
        public string IdVenue { get; set; }
        public string IdGradePathway { get; set; }
        public virtual MsVenue Venue { get; set; }
        public virtual MsGradePathwayClassroom GradePathwayClassroom { get; set; }
        public virtual MsGrade Grade { get; set; }
        public virtual MsAcademicYear AcademicYear { get; set; }
        public virtual MsGradePathway GradePathway { get; set; }
        public virtual ICollection<TrPublishSurveyGrade> PublishSurveyGrades { get; set; }
        public virtual ICollection<MsHomeroomStudent> HomeroomStudents { get; set; }

    }

    internal class MsHomeRoomConfiguration : AuditEntityConfiguration<MsHomeroom>
    {
        public override void Configure(EntityTypeBuilder<MsHomeroom> builder)
        {

            builder.HasOne(x => x.AcademicYear)
             .WithMany(x => x.Homerooms)
             .HasForeignKey(fk => fk.IdAcademicYear)
             .HasConstraintName("FK_MsHomeroom_MsAcademicYear")
             .OnDelete(DeleteBehavior.Restrict)
             .IsRequired();

            builder.HasOne(x => x.Grade)
            .WithMany(x => x.Homerooms)
            .HasForeignKey(fk => fk.IdGrade)
            .HasConstraintName("FK_MsHomeroom_MsGrade")
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

            builder.HasOne(x => x.GradePathway)
            .WithMany(x => x.Homerooms)
            .HasForeignKey(fk => fk.IdGradePathway)
            .HasConstraintName("FK_MsHomeroom_MsGradePathway")
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

            builder.HasOne(x => x.GradePathwayClassroom)
               .WithMany(x => x.Homerooms)
               .HasForeignKey(fk => fk.IdGradePathwayClassRoom)
               .HasConstraintName("FK_MsHomeroom_MsGradePathwayClassroom")
               .OnDelete(DeleteBehavior.Restrict)
               .IsRequired();


            builder.HasOne(x => x.Venue)
              .WithMany(x => x.Homerooms)
              .HasForeignKey(fk => fk.IdVenue)
              .HasConstraintName("FK_MsHomeroom_MsVenue")
              .OnDelete(DeleteBehavior.Restrict);

            builder.Property(x => x.Semester)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
