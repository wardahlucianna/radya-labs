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
    public class MsSubjectSelectionGroupBlock : AuditEntity, IStudentEntity
    {
        public string IdMappingCurriculumSubjectGroup { get; set; }
        public string CurrentIDSubject { get; set; }
        public string IdMappingCurriculumSGBlock { get; set; }
        public string BlockIDSubject { get; set; }
        public virtual MsMappingCurriculumSubjectGroup MappingCurriculumSubjectGroup { get; set; }
    }

    internal class MsSubjectSelectionGroupBlockConfiguration : AuditEntityConfiguration<MsSubjectSelectionGroupBlock>
    {
        public override void Configure(EntityTypeBuilder<MsSubjectSelectionGroupBlock> builder)
        {
            builder.HasOne(x => x.MappingCurriculumSubjectGroup)
                .WithMany(x => x.SubjectSelectionGroupBlocks)
                .HasForeignKey(x => x.IdMappingCurriculumSubjectGroup)
                .HasConstraintName("FK_MsSubjectSelectionGroupBlock_MsMappingCurriculumSubjectGroup")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            base.Configure(builder);
        }
    }
}