using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities.School;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinusSchool.Student.DAL.Entities
{
    public class MsMappingCurriculumGrade : AuditEntity, IStudentEntity
    {
        public string IdSubjectSelectionCurriculum { get; set; }
        public string IdGrade { get; set; }
        public string? Description { get; set; }
        public int? MinSubjectSelection { get; set; }
        public int? MaxSubjectSelection { get; set; }
        public string CurriculumGroup { get; set; }
        public virtual LtSubjectSelectionCurriculum SubjectSelectionCurriculum { get; set; }
        public virtual MsGrade Grade { get; set; }
        public virtual ICollection<MsMappingCurriculumGradeSubLevel> MappingCurriculumGradeSubLevels { get; set; }
        public virtual ICollection<MsMappingCurriculumSubjectGroup> MappingCurriculumSubjectGroups { get; set; }
    }

    internal class MsMappingCurriculumGradeConfiguration : AuditEntityConfiguration<MsMappingCurriculumGrade>
    {
        public override void Configure(EntityTypeBuilder<MsMappingCurriculumGrade> builder)
        {
            builder.HasOne(x => x.SubjectSelectionCurriculum)
                .WithMany(x => x.MappingCurriculumGrades)
                .HasForeignKey(x => x.IdSubjectSelectionCurriculum)
                .HasConstraintName("FK_MsMappingCurriculumGrade_LtSubjectSelectionCurriculum")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder.HasOne(x => x.Grade)
                .WithMany(x => x.MappingCurriculumGrades)
                .HasForeignKey(x => x.IdGrade)
                .HasConstraintName("FK_MsMappingCurriculumGrade_MsGrade")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            base.Configure(builder);
        }
    }
}