using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using BinusSchool.Persistence.SchedulingDb.Entities.Student;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchedulingDb.Entities
{
    public class TrStudentProgramme : AuditEntity, ISchedulingEntity
    {
        public StudentProgrammeEnum Programme { get; set; }
        public DateTime StartDate { get; set; }
        public bool IsSendEmail { get; set; }
        public string IdStudent { get; set; }
        public string IdSchool { get; set; }
        public virtual MsSchool School { get; set; }
        public virtual MsStudent Student { get; set; }
        public virtual ICollection<HTrStudentProgramme> HTrStudentProgrammes { get; set; }
    }

    internal class TrHomeroomStudentEnrollmentProgrammeConfiguration : AuditEntityConfiguration<TrStudentProgramme>
    {
        public override void Configure(EntityTypeBuilder<TrStudentProgramme> builder)
        {
            builder.Property(e => e.Programme)
                .HasMaxLength(maxLength: 10)
                .HasConversion(valueToDb =>
                valueToDb.ToString(),
                valueFromDb => (StudentProgrammeEnum)Enum.Parse(typeof(StudentProgrammeEnum), valueFromDb))
                .IsRequired();

            builder.HasOne(x => x.School)
               .WithMany(x => x.StudentProgrammes)
               .HasForeignKey(fk => fk.IdSchool)
               .HasConstraintName("FK_TrStudentProgramme_MsSchool")
               .OnDelete(DeleteBehavior.Restrict)
               .IsRequired();

            builder.HasOne(x => x.Student)
               .WithMany(x => x.StudentProgrammes)
               .HasForeignKey(fk => fk.IdStudent)
               .HasConstraintName("FK_TrStudentProgramme_MsStudent")
               .OnDelete(DeleteBehavior.Restrict)
               .IsRequired();
            base.Configure(builder);
        }
    }
}
