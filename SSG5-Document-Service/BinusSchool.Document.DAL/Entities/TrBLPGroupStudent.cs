using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.DocumentDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BinusSchool.Persistence.DocumentDb.Entities.School;
using BinusSchool.Persistence.DocumentDb.Entities.Student;
using BinusSchool.Persistence.DocumentDb.Entities.Scheduling;
using System;

namespace BinusSchool.Persistence.DocumentDb.Entities
{
    public class TrBLPGroupStudent : AuditEntity, IDocumentEntity
    {
        public string IdStudent { get; set; }
        public string IdAcademicYear { get; set; }
        public string IdBLPGroup { get; set; }
        public int Semester { get; set; }
        public string IdBLPStatus { get; set; }
        public string IdHomeroomStudent { get; set; }
        public DateTime? HardCopySubmissionDate { get; set; }

        public virtual MsAcademicYear AcademicYear { get; set; }
        public virtual MsStudent Student { get; set; }
        public virtual LtBLPGroup BLPGroup { get; set; }
        public virtual LtBLPStatus BLPStatus { get; set; }
        public virtual MsHomeroomStudent HomeroomStudent { get; set; }
    }

    internal class TrBLPGroupStudentConfiguration : AuditEntityConfiguration<TrBLPGroupStudent>
    {
        public override void Configure(EntityTypeBuilder<TrBLPGroupStudent> builder)
        {
            builder.Property(x => x.IdStudent)
                .HasMaxLength(36)
                .IsRequired();

            builder.Property(x => x.IdAcademicYear)
                .HasMaxLength(36)
                .IsRequired();

            builder.Property(x => x.IdBLPGroup)
                .HasMaxLength(36);
           
            builder.Property(x => x.Semester)
                .IsRequired();

            builder.Property(x => x.IdBLPStatus)
                .HasMaxLength(36)
                .IsRequired();

            builder.Property(x => x.IdHomeroomStudent)
               .HasMaxLength(36)
               .IsRequired();


            builder.HasOne(x => x.AcademicYear)
                .WithMany(x => x.BLPGroupStudents)
                .HasForeignKey(fk => fk.IdAcademicYear)
                .HasConstraintName("FK_TrBLPGroupStudent_MsAcademicYear")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder.HasOne(x => x.Student)
                .WithMany(x => x.BLPGroupStudents)
                .HasForeignKey(fk => fk.IdStudent)
                .HasConstraintName("FK_TrBLPGroupStudent_MsStudent")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder.HasOne(x => x.BLPGroup)
                .WithMany(x => x.BLPGroupStudents)
                .HasForeignKey(fk => fk.IdBLPGroup)
                .HasConstraintName("FK_TrBLPGroupStudent_LtBLPGroup")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder.HasOne(x => x.BLPStatus)
               .WithMany(x => x.BLPGroupStudents)
               .HasForeignKey(fk => fk.IdBLPStatus)
               .HasConstraintName("FK_TrBLPGroupStudent_LtBLPStatus")
               .OnDelete(DeleteBehavior.Restrict);
      

            builder.HasOne(x => x.HomeroomStudent)
                .WithMany(x => x.BLPGroupStudents)
                .HasForeignKey(fk => fk.IdHomeroomStudent)
                .HasConstraintName("FK_TrBLPGroupStudent_MsHomeroomStudent")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
