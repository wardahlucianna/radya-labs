using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Persistence.StudentDb.Entities.School;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinusSchool.Student.DAL.Entities
{
    public class TrStudentSubjectSelectionMajor : AuditEntity, IStudentEntity
    {
        public string IdMappingMajorGrade { get; set; }
        public string IdStudent { get; set; }
        public string? IdSubjectSelectionPeriod { get; set; }
        public string IdCurrentGrade { get; set; }
        public string IdAcademicYear { get; set; }
        public string? OtherSelectionMajor { get; set; }
        public string IdUserAction { get; set; }
        public virtual MsMappingMajorGrade MappingMajorGrade { get; set; }
        public virtual MsStudent Student { get; set; }
        public virtual MsSubjectSelectionPeriod SubjectSelectionPeriod { get; set; }
        public virtual MsGrade CurrentGrade { get; set; }
        public virtual MsAcademicYear AcademicYear { get; set; }
    }

    internal class TrStudentSubjectSelectionMajorConfiguration : AuditEntityConfiguration<TrStudentSubjectSelectionMajor>
    {
        public override void Configure(EntityTypeBuilder<TrStudentSubjectSelectionMajor> builder)
        {
            builder.HasOne(x => x.MappingMajorGrade)
                .WithMany(x => x.StudentSubjectSelectionMajors)
                .HasForeignKey(x => x.IdMappingMajorGrade)
                .HasConstraintName("FK_TrStudentSubjectSelectionMajor_MsMappingMajorGrade")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder.HasOne(x => x.Student)
                .WithMany(x => x.StudentSubjectSelectionMajors)
                .HasForeignKey(x => x.IdStudent)
                .HasConstraintName("FK_TrStudentSubjectSelectionMajor_MsStudent")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder.HasOne(x => x.SubjectSelectionPeriod)
                .WithMany(x => x.StudentSubjectSelectionMajors)
                .HasForeignKey(x => x.IdSubjectSelectionPeriod)
                .HasConstraintName("FK_TrStudentSubjectSelectionMajor_MsSubjectSelectionPeriod")
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.CurrentGrade)
                .WithMany(x => x.StudentSubjectSelectionMajors)
                .HasForeignKey(x => x.IdCurrentGrade)
                .HasConstraintName("FK_TrStudentSubjectSelectionMajor_MsGrade")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder.HasOne(x => x.AcademicYear)
                .WithMany(x => x.StudentSubjectSelectionMajors)
                .HasForeignKey(x => x.IdAcademicYear)
                .HasConstraintName("FK_TrStudentSubjectSelectionMajor_MsAcademicYear")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            base.Configure(builder);
        }
    }
}