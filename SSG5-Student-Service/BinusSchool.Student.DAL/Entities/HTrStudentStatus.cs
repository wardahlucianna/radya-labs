using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities.School;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities
{
    public class HTrStudentStatus : AuditNoUniqueEntity, IStudentEntity
    {
        public string IdHTrStudentStatus { get; set; }
        public string IdTrStudentStatus { get; set; }
        public string IdAcademicYear { get; set; }
        public string IdStudent { get; set; }
        public int IdStudentStatus { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string CurrentStatus { get; set; }
        public string Remarks { get; set; }
        public bool ActiveStatus { get; set; }
        public virtual TrStudentStatus TrStudentStatus { get; set; }
        public virtual MsAcademicYear AcademicYear { get; set; }
        public virtual MsStudent Student { get; set; }
        public virtual LtStudentStatus StudentStatus { get; set; }
    }

    internal class HTrStudentStatusConfiguration : AuditNoUniqueEntityConfiguration<HTrStudentStatus>
    {
        public override void Configure(EntityTypeBuilder<HTrStudentStatus> builder)
        {
            builder.HasKey(x => x.IdHTrStudentStatus);

            builder.Property(x => x.IdHTrStudentStatus)
               .HasMaxLength(36)
               .IsRequired();

            builder.Property(x => x.IdTrStudentStatus)
                .HasMaxLength(36)
                .IsRequired();

            builder.Property(x => x.IdAcademicYear)
               .HasMaxLength(36)
               .IsRequired();

            builder.Property(x => x.IdStudent)
               .HasMaxLength(36)
               .IsRequired();

            builder.Property(x => x.StartDate)
                .HasColumnType(typeName: "datetime2")
                .IsRequired();

            builder.Property(x => x.EndDate)
                .HasColumnType(typeName: "datetime2");

            builder.Property(x => x.CurrentStatus)
                .HasColumnType("CHAR(1)")
                .IsRequired();

            builder.Property(x => x.Remarks)
                .HasMaxLength(1000);

            builder.HasOne(x => x.TrStudentStatus)
             .WithMany(y => y.HTrStudentStatuss)
             .HasForeignKey(fk => fk.IdTrStudentStatus)
             .HasConstraintName("FK_HTrStudentStatus_TrStudentStatus")
             .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.AcademicYear)
              .WithMany(y => y.HTrStudentStatuss)
              .HasForeignKey(fk => fk.IdAcademicYear)
              .HasConstraintName("FK_HTrStudentStatus_MsAcademicYear")
              .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.Student)
                .WithMany(y => y.HTrStudentStatuss)
                .HasForeignKey(fk => fk.IdStudent)
                .HasConstraintName("FK_HTrStudentStatus_MsStudent")
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.StudentStatus)
               .WithMany(y => y.HTrStudentStatuss)
               .HasForeignKey(fk => fk.IdStudentStatus)
               .HasConstraintName("FK_HTrStudentStatus_LTStudentStatus")
               .OnDelete(DeleteBehavior.NoAction);
        }

    }
}
