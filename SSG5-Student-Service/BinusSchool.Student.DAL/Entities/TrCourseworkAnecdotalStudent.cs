using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities.School;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities
{
    public class TrCourseworkAnecdotalStudent : AuditEntity, IStudentEntity
    {
        public string IdStudent { get; set; }
        public string IdUIO { get; set; }
        public string Content { get; set; }
        public int Type { get; set; }
        public bool NotifyParentStudent { get; set; }
        public string IdAcademicYear { get; set; }
        public int Semester { get; set; }

        public virtual MsStudent Student { get; set; }
        public virtual MsUOI UOI { get; set; }
        public virtual ICollection<TrCourseworkAnecdotalAttachment> Attachments { get; set; }
        public virtual ICollection<TrCourseworkAnecdotalStudentComment> Comments { get; set; }
        public virtual ICollection<TrCourseworkAnecdotalStudentSeen> Seens { get; set; }
        public virtual MsAcademicYear AcademicYear { get; set; }
    }

    internal class TrCourseworkAnecdotalStudentConfiguration : AuditEntityConfiguration<TrCourseworkAnecdotalStudent>
    {
        public override void Configure(EntityTypeBuilder<TrCourseworkAnecdotalStudent> builder)
        {        
            builder.Property(x => x.Content)
                .IsRequired(true)
                .HasMaxLength(int.MaxValue);

            builder.HasOne(x => x.UOI)
                .WithMany(x => x.CourseworkAnecdotalStudents)
                .HasForeignKey(fk => fk.IdUIO)
                .HasConstraintName("FK_TrCourseworkAnecdotal_MsUIO")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired(false);

            builder.HasOne(x => x.Student)
                .WithMany(x => x.CourseworkAnecdotalStudents)
                .HasForeignKey(fk => fk.IdStudent)
                .HasConstraintName("FK_TrCourseworkAnecdotal_Student")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder.HasOne(x => x.AcademicYear)
                .WithMany(x => x.CourseworkAnecdotalStudents)
                .HasForeignKey(fk => fk.IdAcademicYear)
                .HasConstraintName("FK_TrCourseworkAnecdotal_MsAcademicYear")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            base.Configure(builder); 

        }
    }

}
