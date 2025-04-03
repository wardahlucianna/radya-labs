using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.WorkflowDb.Abstractions;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.WorkflowDb.Entities.School
{
    public class MsSchool : AuditEntity, IWorkflowEntity
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Address { get; set; }
        public virtual ICollection<MsApprovalWorkflow> ApprovalWorkflows { get; set; }

        //public virtual ICollection<MsAcademicYear> AcademicYears { get; set; }
        //public virtual ICollection<MsBuilding> Buildings { get; set; }
        //public virtual ICollection<MsCurriculum> Curriculums { get; set; }
        //public virtual ICollection<MsDivision> Divisions { get; set; }
        //public virtual ICollection<MsClassroom> Classrooms { get; set; }
        //public virtual ICollection<MsSessionSet> SessionSets { get; set; }
        //public virtual ICollection<MsSubjectType> SubjectTypes { get; set; }
        //public virtual ICollection<MsSubjectGroup> SubjectGroups { get; set; }
        //public virtual ICollection<MsSubjectLevel> SubjectLevels { get; set; }
    }

    internal class MsSchoolConfiguration : AuditEntityConfiguration<MsSchool>
    {
        public override void Configure(EntityTypeBuilder<MsSchool> builder)
        {
            builder.Property(x => x.Name)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(x => x.Description)
                .HasMaxLength(128)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
