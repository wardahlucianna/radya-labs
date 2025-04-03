using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities.Scheduling;
using BinusSchool.Student.DAL.Entities.School;
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
    public class MsMappingCurriculumSubjectGroupDtl : AuditEntity, IStudentEntity
    {
        public string IdMappingCurriculumSubjectGroup { get; set; }
        public string IdSubject { get; set; }
        public string IdSubjectLevel { get; set; }
        public bool IsRequired { get; set; }
        public virtual MsMappingCurriculumSubjectGroup MappingCurriculumSubjectGroup { get; set; }
        public virtual MsSubject Subject { get; set; }
        public virtual MsSubjectLevel SubjectLevel { get; set; }
        public virtual ICollection<TrStudentSubjectSelection> StudentSubjectSelections { get; set; }
    }

    internal class MsMappingCurriculumSubjectGroupDtlConfiguration : AuditEntityConfiguration<MsMappingCurriculumSubjectGroupDtl>
    {
        public override void Configure(EntityTypeBuilder<MsMappingCurriculumSubjectGroupDtl> builder)
        {
            builder.HasOne(x => x.MappingCurriculumSubjectGroup)
                .WithMany(x => x.MappingCurriculumSubjectGroupDtls)
                .HasForeignKey(x => x.IdMappingCurriculumSubjectGroup)
                .HasConstraintName("FK_MsMappingCurriculumSubjectGroupDtl_MsMappingCurriculumSubjectGroup")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder.HasOne(x => x.Subject)
                .WithMany(x => x.MappingCurriculumSubjectGroupDtls)
                .HasForeignKey(x => x.IdSubject)
                .HasConstraintName("FK_MsMappingCurriculumSubjectGroupDtl_MsSubject")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder.HasOne(x => x.SubjectLevel)
                .WithMany(x => x.MappingCurriculumSubjectGroupDtls)
                .HasForeignKey(x => x.IdSubjectLevel)
                .HasConstraintName("FK_MsMappingCurriculumSubjectGroupDtl_MsSubjectLevel")
                .OnDelete(DeleteBehavior.NoAction);

            base.Configure(builder);
        }
    }
}
