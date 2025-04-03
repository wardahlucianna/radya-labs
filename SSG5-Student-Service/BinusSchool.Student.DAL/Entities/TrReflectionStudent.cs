using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities.School;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities
{
    public class TrReflectionStudent : AuditEntity, IStudentEntity
    {
        public string IdStudent { get; set; }
        public string Content { get; set; }
        public string IdAcademicYear { get; set; }
        public int Semester { get; set; }
        public virtual MsStudent Student { get; set; }
        public virtual ICollection<TrReflectionStudentComment> Comments { get; set; }
        public virtual MsAcademicYear AcademicYear { get; set; }
        public virtual ICollection<TrReflectionStudentAttachment> ReflectionStudentAttachments { get; set; }
    }

    internal class TrReflectionStudentConfiguration : AuditEntityConfiguration<TrReflectionStudent>
    {
        public override void Configure(EntityTypeBuilder<TrReflectionStudent> builder)
        {        
            builder.Property(x => x.Content)
                .IsRequired(true)
                .HasMaxLength(int.MaxValue);

            builder.HasOne(x => x.Student)
                .WithMany(x => x.ReflectionStudent)
                .HasForeignKey(fk => fk.IdStudent)
                .HasConstraintName("FK_TrReflectionStudent_Student")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder.HasOne(x => x.AcademicYear)
                .WithMany(x => x.ReflectionStudents)
                .HasForeignKey(fk => fk.IdAcademicYear)
                .HasConstraintName("FK_TrReflectionStudent_MsAcademicYear")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            base.Configure(builder); 

        }
    }
}
