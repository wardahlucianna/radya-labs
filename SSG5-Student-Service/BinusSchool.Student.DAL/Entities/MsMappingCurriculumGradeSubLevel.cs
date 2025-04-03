using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Student.DAL.Entities.School;
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
    public class MsMappingCurriculumGradeSubLevel : AuditEntity, IStudentEntity
    {
        public string IdMappingCurriculumGrade { get; set; }
        public string IdSubjectLevel { get; set; }
        public int? MinRange { get; set; }
        public int? MaxRange { get; set; }
        public virtual MsMappingCurriculumGrade MappingCurriculumGrade { get; set; }
        public virtual MsSubjectLevel SubjectLevel { get; set; }
    }

    internal class MsMappingCurriculumGradeSubLevelConfiguration : AuditEntityConfiguration<MsMappingCurriculumGradeSubLevel>
    {
        public override void Configure(EntityTypeBuilder<MsMappingCurriculumGradeSubLevel> builder)
        {
            builder.HasOne(x => x.MappingCurriculumGrade)
                .WithMany(x => x.MappingCurriculumGradeSubLevels)
                .HasForeignKey(x => x.IdMappingCurriculumGrade)
                .HasConstraintName("FK_MsMappingCurriculumGradeSubLevel_MsMappingCurriculumGrade")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder.HasOne(x => x.SubjectLevel)
                .WithMany(x => x.MappingCurriculumGradeSubLevels)
                .HasForeignKey(x => x.IdSubjectLevel)
                .HasConstraintName("FK_MsMappingCurriculumGradeSubLevel_MsSubjectLevel")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
