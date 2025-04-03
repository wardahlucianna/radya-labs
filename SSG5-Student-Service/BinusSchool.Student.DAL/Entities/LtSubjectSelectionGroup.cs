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
    public class LtSubjectSelectionGroup : AuditEntity, IStudentEntity
    {
        public string IdSchool { get; set; }
        public string SubjectSelectionGroupName { get; set; }
        public bool ActiveStatus { get; set; }
        public virtual MsSchool School { get; set; }
        public virtual ICollection<MsMappingCurriculumSubjectGroup> MappingCurriculumSubjectGroups { get; set; }
    }

    internal class LtSubjectSelectionGroupConfiguration : AuditEntityConfiguration<LtSubjectSelectionGroup>
    {
        public override void Configure(EntityTypeBuilder<LtSubjectSelectionGroup> builder)
        {
            builder.HasOne(x => x.School)
                .WithMany(x => x.SubjectSelectionGroups)
                .HasForeignKey(x => x.IdSchool)
                .HasConstraintName("FK_LtSubjectSelectionGroup_MsSchool")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            base.Configure(builder);
        }
    }
}