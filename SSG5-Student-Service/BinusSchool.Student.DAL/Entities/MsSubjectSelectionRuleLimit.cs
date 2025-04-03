using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
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
    public class MsSubjectSelectionRuleLimit : AuditEntity, IStudentEntity
    {
        public int LimitUpdateNewStudent { get; set; }
        public int LimitUpdateExisitingStudent { get; set; }
        public string IdGrade { get; set; }
        public string? IdMappingCurriculumSubjectGroup { get; set; }
        public virtual MsGrade Grade { get; set; }
        public virtual MsMappingCurriculumSubjectGroup MappingCurriculumSubjectGroup { get; set; }

    }

    internal class MsSubjectSelectionRuleLimitConfiguration : AuditEntityConfiguration<MsSubjectSelectionRuleLimit>
    {
        public override void Configure(EntityTypeBuilder<MsSubjectSelectionRuleLimit> builder)
        {
            builder.HasOne(x => x.Grade)
                .WithMany(x => x.SubjectSelectionRuleLimits)
                .HasForeignKey(x => x.IdGrade)
                .HasConstraintName("FK_MsSubjectSelectionRuleLimit_MsGrade")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder.HasOne(x => x.MappingCurriculumSubjectGroup)
                .WithMany(x => x.SubjectSelectionRuleLimits)
                .HasForeignKey(x => x.IdMappingCurriculumSubjectGroup)
                .HasConstraintName("FK_MsSubjectSelectionRuleLimit_MsMappingCurriculumSubjectGroup")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            base.Configure(builder);
        }
    }
}