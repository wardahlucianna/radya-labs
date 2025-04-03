using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.DocumentDb.Abstractions;
using BinusSchool.Persistence.DocumentDb.Entities.School;
using BinusSchool.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.DocumentDb.Entities.Scheduling
{
    public class MsHomeroom : AuditEntity, IDocumentEntity
    {
        public string IdAcademicYear { get; set; }
        public string IdGrade { get; set; }
        public string IdGradePathwayClassRoom { get; set; }
        public string IdVenue { get; set; }
        public string IdGradePathway { get; set; }
        public int Semester { get; set; }

        public virtual MsAcademicYear AcademicYear { get; set; }
        public virtual MsGrade Grade { get; set; }
        public virtual MsGradePathway GradePathway { get; set; }
        public virtual MsGradePathwayClassroom GradePathwayClassroom { get; set; }
        public virtual ICollection<MsHomeroomStudent> HomeroomStudents { get; set; }
        public virtual ICollection<MsDocumentReqApplicant> DocumentReqApplicants { get; set; }
        public virtual ICollection<MsHomeroomTeacher> HomeroomTeachers { get; set; }
    }

    internal class MsHomeRoomConfiguration : AuditEntityConfiguration<MsHomeroom>
    {
        public override void Configure(EntityTypeBuilder<MsHomeroom> builder)
        {
            builder.Property(x => x.IdAcademicYear)
                .HasMaxLength(36)
                .IsRequired();

            builder.Property(x => x.IdVenue)
                .HasMaxLength(36);

            builder.Property(x => x.Semester)
                .IsRequired();

            builder.Property(x => x.IdGrade)
                .HasMaxLength(36)
                .IsRequired();

            builder.Property(x => x.IdGradePathwayClassRoom)
                .HasMaxLength(36)
                .IsRequired();

            //builder.Property(x => x.IdVenue)
            //    .HasMaxLength(36)
            //    .IsRequired();

            builder.Property(x => x.IdGradePathway)
               .HasMaxLength(36)
               .IsRequired();

            builder.HasOne(x => x.AcademicYear)
               .WithMany(x => x.Homerooms)
               .HasForeignKey(fk => fk.IdAcademicYear)
               .HasConstraintName("FK_MsHomeroom_MsAcademicYear")
               .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Grade)
               .WithMany(x => x.Homerooms)
               .HasForeignKey(fk => fk.IdGrade)
               .HasConstraintName("FK_MsHomeroom_MsGrade")
               .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.GradePathway)
              .WithMany(x => x.Homerooms)
              .HasForeignKey(fk => fk.IdGradePathway)
              .HasConstraintName("FK_MsHomeroom_MsGradePathway")
              .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.GradePathwayClassroom)
               .WithMany(x => x.Homerooms)
               .HasForeignKey(fk => fk.IdGradePathwayClassRoom)
               .HasConstraintName("FK_MsHomeroom_MsGradePathwayClassRoom")
               .OnDelete(DeleteBehavior.NoAction);

            base.Configure(builder);
        }
    }
}
