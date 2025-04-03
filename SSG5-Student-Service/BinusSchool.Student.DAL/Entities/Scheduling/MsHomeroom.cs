using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.Scheduling;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities.School;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities.Scheduling
{
    public class MsHomeroom : AuditEntity, IStudentEntity
    {
        public string IdGradePathwayClassRoom { get; set; }
        public int Semester { get; set; }
        public string IdGrade { get; set; }
        public virtual MsGradePathwayClassroom MsGradePathwayClassroom { get; set; }
        public virtual ICollection<MsHomeroomStudent> HomeroomStudents { get; set; }
        public virtual ICollection<MsHomeroomPathway> HomeroomPathways { get; set; }
        public virtual ICollection<MsHomeroomTeacher> HomeroomTeachers { get; set; }
        public virtual MsGrade Grade { get; set; }
        public virtual ICollection<TrExemplaryStudent> ExemplaryStudents { get; set; }
        public virtual ICollection<TrStudentLockerReservation> StudentLockerReservations { get; set; }
        public virtual ICollection<TrStudentExit> StudentExits { get; set; }
    }

    internal class MsHomeroomConfiguration : AuditEntityConfiguration<MsHomeroom>
    {
        public override void Configure(EntityTypeBuilder<MsHomeroom> builder)
        {
            builder.Property(x => x.IdGradePathwayClassRoom)
                .HasMaxLength(36)
                .IsRequired();

            builder.HasOne(x => x.MsGradePathwayClassroom)
               .WithMany(x => x.MsHomerooms)
                .IsRequired()
               .HasForeignKey(fk => fk.IdGradePathwayClassRoom)
               .HasConstraintName("FK_MsHomeroom_MsGradePathwayClassroom")
               .OnDelete(DeleteBehavior.Cascade);

            
            builder.HasOne(x => x.Grade)
                .WithMany(x => x.Homerooms)
                .HasForeignKey(fk => fk.IdGrade)
                .HasConstraintName("FK_MsHomeroom_MsGrade")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();   

            base.Configure(builder);
        }
    }
}
