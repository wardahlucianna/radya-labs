using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.TeachingDb.Abstractions;
using BinusSchool.Persistence.TeachingDb.Entities.School;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.TeachingDb.Entities.Scheduling
{
    public class MsHomeroom : AuditEntity, ITeachingEntity
    {
        public string IdAcademicYear { get; set; }
        public int Semester { get; set; }
        public string IdGrade { get; set; }
        public string IdGradePathwayClassRoom { get; set; }
        public string IdVenue { get; set; }
        public string IdGradePathway { get; set; }

        public virtual ICollection<MsHomeroomTeacher> HomeroomTeachers { get; set; }
        public virtual ICollection<MsHomeroomPathway> HomeroomPathways { get; set; }
        public virtual MsAcademicYear AcademicYear { get; set; }
        public virtual MsGrade Grade { get; set; }
        public virtual MsGradePathwayClassroom GradePathwayClassroom { get; set; }

        public virtual MsVenue Venue { get; set; }
        public virtual MsGradePathway GradePathway { get; set; }
    }

    internal class MsHomeRoomConfiguration : AuditEntityConfiguration<MsHomeroom>
    {
        public override void Configure(EntityTypeBuilder<MsHomeroom> builder)
        {
            builder.HasOne(x => x.AcademicYear)
               .WithMany(x => x.Homerooms)
               .HasForeignKey(fk => fk.IdAcademicYear)
               .HasConstraintName("FK_MsHomeroom_MsAcademicYear")
               .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder.Property(x => x.Semester)
                .IsRequired();

            builder.HasOne(x => x.Grade)
                .WithMany(x => x.Homerooms)
                .HasForeignKey(fk => fk.IdGrade)
                .HasConstraintName("FK_MsHomeroom_MsGrade")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder.HasOne(x => x.GradePathwayClassroom)
                 .WithMany(x => x.Homerooms)
                 .HasForeignKey(fk => fk.IdGradePathwayClassRoom)
                 .HasConstraintName("FK_MsHomeroom_MsGradePathwayClassrooom")
                 .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();


            builder.HasOne(x => x.Venue)
                 .WithMany(x => x.Homerooms)
                 .HasForeignKey(fk => fk.IdVenue)
                 .HasConstraintName("FK_MsHomeroom_MsVenue")
                 .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.GradePathway)
               .WithMany(x => x.Homerooms)
               .HasForeignKey(fk => fk.IdGradePathway)
               .HasConstraintName("FK_MsHomeroom_MsGradePathway")
               .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();


            base.Configure(builder);
        }
    }
}
