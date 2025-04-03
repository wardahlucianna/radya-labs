using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
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
    public class MsMappingCurriculumSubjectGroup : AuditEntity, IStudentEntity
    {
        public string IdMappingCurriculumGrade { get; set; }
        public string IdSubjectSelectionGroup { get; set; }
        public string Description { get; set; }
        public int MinSubjectGroup { get; set; }
        public int MaxSubjectGroup { get; set; }
        public bool UsePriority { get; set; }
        public bool IsPackage { get; set; }
        public bool ActiveStatus { get; set; }
        public virtual MsMappingCurriculumGrade MappingCurriculumGrade { get; set; }
        public virtual LtSubjectSelectionGroup SubjectSelectionGroup { get; set; }
        public virtual ICollection<MsSubjectSelectionRuleLimit> SubjectSelectionRuleLimits { get; set; }
        public virtual ICollection<MsSubjectSelectionGroupBlock> SubjectSelectionGroupBlocks { get; set; }
        public virtual ICollection<MsMappingCurriculumSubjectGroupDtl> MappingCurriculumSubjectGroupDtls { get; set; }
    }

    internal class MsMappingCurriculumSubjectGroupConfiguration : AuditEntityConfiguration<MsMappingCurriculumSubjectGroup>
    {
        public override void Configure(EntityTypeBuilder<MsMappingCurriculumSubjectGroup> builder)
        {
            builder.HasOne(x => x.MappingCurriculumGrade)
                .WithMany(x => x.MappingCurriculumSubjectGroups)
                .HasForeignKey(x => x.IdMappingCurriculumGrade)
                .HasConstraintName("FK_MsMappingCurriculumSubjectGroup_MsMappingCurriculumGrade")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder.HasOne(x => x.SubjectSelectionGroup)
                .WithMany(x => x.MappingCurriculumSubjectGroups)
                .HasForeignKey(x => x.IdSubjectSelectionGroup)
                .HasConstraintName("FK_MsMappingCurriculumSubjectGroup_LtSubjectSelectionGroup")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            base.Configure(builder);
        }
    }
}