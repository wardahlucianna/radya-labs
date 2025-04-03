using System;
using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities.School;
using BinusSchool.Persistence.StudentDb.Entities.Scheduling;
using BinusSchool.Persistence.StudentDb.Entities.User;
using BinusSchool.Common.Model.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities
{
    public class TrExperienceStudent : AuditEntity, IStudentEntity
    {
        public string IdAcademicYear { get; set; }
        public string IdStudent { get; set; }
        public StatusOverallExperienceStudent StatusOverall { get; set; }
        public virtual MsAcademicYear AcademicYear { get; set; }
        public virtual MsStudent Student { get; set; }
    }

    internal class TrExperienceStudentConfiguration : AuditEntityConfiguration<TrExperienceStudent>
    {
        public override void Configure(EntityTypeBuilder<TrExperienceStudent> builder)
        {
            builder.Property(x => x.IdAcademicYear)
                .HasMaxLength(36)
                .IsRequired();

            builder.Property(x => x.IdStudent)
                .HasMaxLength(36)
                .IsRequired();

            builder.Property(e => e.StatusOverall)
                .HasMaxLength(maxLength: 10)
                .HasConversion(valueToDb =>
                valueToDb.ToString(),
                valueFromDb => (StatusOverallExperienceStudent)Enum.Parse(typeof(StatusOverallExperienceStudent), valueFromDb))
                .IsRequired();

            builder.HasOne(x => x.AcademicYear)
                .WithMany(x => x.TrExperienceStudents)
                .IsRequired()
                .HasForeignKey(fk => fk.IdAcademicYear)
                .HasConstraintName("FK_TrExperienceStudent_MsAcademicYear")
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Student)
                .WithMany(x => x.TrExperienceStudents)
                .IsRequired()
                .HasForeignKey(fk => fk.IdStudent)
                .HasConstraintName("FK_TrExperienceStudent_MsStudent")
                .OnDelete(DeleteBehavior.Cascade);

            base.Configure(builder);
        }
    }
}
