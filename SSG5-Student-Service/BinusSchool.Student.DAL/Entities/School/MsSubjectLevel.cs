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

namespace BinusSchool.Student.DAL.Entities.School
{
    public class MsSubjectLevel : CodeEntity, IStudentEntity
    {
        public string IdSchool { get; set; }
        public virtual MsSchool School { get; set; }
        public virtual ICollection<MsMappingCurriculumGradeSubLevel> MappingCurriculumGradeSubLevels { get; set; }
        public virtual ICollection<MsMappingCurriculumSubjectGroupDtl> MappingCurriculumSubjectGroupDtls { get; set; }
        public virtual ICollection<MsSubjectMappingSubjectLevel> SubjectMappingSubjectLevels { get; set; }

    }

    internal class MsSubjectLevelConfiguration : CodeEntityConfiguration<MsSubjectLevel>
    {
        public override void Configure(EntityTypeBuilder<MsSubjectLevel> builder)
        {
            builder.HasOne(x => x.School)
                .WithMany(x => x.SubjectLevels)
                .HasForeignKey(fk => fk.IdSchool)
                .HasConstraintName("FK_MsSubjectLevel_MsSchool")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            base.Configure(builder);
        }
    }
}