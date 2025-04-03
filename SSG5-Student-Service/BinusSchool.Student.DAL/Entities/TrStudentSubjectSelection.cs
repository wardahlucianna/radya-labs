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
    public class TrStudentSubjectSelection : AuditEntity, IStudentEntity
    {
        public string IdMappingCurriculumSubjectGroupDtl { get; set; }
        public string IdAcademicYear { get; set; }
        public string IdStudent { get; set; }
        public string? IdSubjectSelectionPeriod { get; set; }
        public string IdCurrentGrade { get; set; }
        public int? SelectionPriorityNo { get; set; }
        public string IdUserAction { get; set; }
        public virtual MsMappingCurriculumSubjectGroupDtl MappingCurriculumSubjectGroupDtl { get; set; }
        public virtual MsAcademicYear AcademicYear { get; set; }
        public virtual MsStudent Student { get; set; }
        public virtual MsSubjectSelectionPeriod SubjectSelectionPeriod { get; set; }
        public virtual MsGrade CurrentGrade { get; set; }
    }

    internal class TrStudentSubjectSelectionConfiguration : AuditEntityConfiguration<TrStudentSubjectSelection>
    {
        public override void Configure(EntityTypeBuilder<TrStudentSubjectSelection> builder)
        {
            builder.HasOne(x => x.MappingCurriculumSubjectGroupDtl)
                .WithMany(x => x.StudentSubjectSelections)
                .HasForeignKey(x => x.IdMappingCurriculumSubjectGroupDtl)
                .HasConstraintName("FK_TrStudentSubjectSelection_MsMappingCurriculumSubjectGroupDtl")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder.HasOne(x => x.AcademicYear)
                .WithMany(x => x.StudentSubjectSelections)
                .HasForeignKey(x => x.IdAcademicYear)
                .HasConstraintName("FK_TrStudentSubjectSelection_MsAcademicYear")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder.HasOne(x => x.Student)
                .WithMany(x => x.StudentSubjectSelections)
                .HasForeignKey(x => x.IdStudent)
                .HasConstraintName("FK_TrStudentSubjectSelection_MsStudent")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder.HasOne(x => x.SubjectSelectionPeriod)
                .WithMany(x => x.StudentSubjectSelections)
                .HasForeignKey(x => x.IdSubjectSelectionPeriod)
                .HasConstraintName("FK_TrStudentSubjectSelection_MsSubjectSelectionPeriod")
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.CurrentGrade)
                .WithMany(x => x.StudentSubjectSelections)
                .HasForeignKey(x => x.IdCurrentGrade)
                .HasConstraintName("FK_TrStudentSubjectSelection_MsGrade")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            base.Configure(builder);
        }
    }
}