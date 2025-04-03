using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities.School;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities
{
    public class MsStudentGrade : AuditEntity, IStudentEntity
    {
        public string IdStudent { get; set; }
        public string IdGrade { get; set; }

        public virtual MsStudent Student { get; set; }
        public virtual MsGrade Grade { get; set; }
        public virtual ICollection<MsStudentGradePathway> StudentGradePathways { get; set; }
    }

    internal class MsStudentGradeConfiguration : AuditEntityConfiguration<MsStudentGrade>
    {
        public override void Configure(EntityTypeBuilder<MsStudentGrade> builder)
        {
            builder.Property(x => x.IdStudent)
                .HasMaxLength(36)
                .IsRequired();
                
            builder.HasOne(x => x.Student)
                .WithMany(x => x.StudentGrades)
                .IsRequired()
                .HasForeignKey(fk => fk.IdStudent)
                .HasConstraintName("FK_MsStudentGrade_MsStudent")
                .OnDelete(DeleteBehavior.Cascade);

              builder.HasOne(x => x.Grade)
                .WithMany(x => x.StudentGrades)
                .IsRequired()
                .HasForeignKey(fk => fk.IdGrade)
                .HasConstraintName("FK_MsStudentGrade_MsGrade")
                .OnDelete(DeleteBehavior.Cascade);
                
            base.Configure(builder);
        }
    }
}
