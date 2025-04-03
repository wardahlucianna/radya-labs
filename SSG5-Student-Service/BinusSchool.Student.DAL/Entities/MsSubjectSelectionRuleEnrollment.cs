using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities.Scheduling;
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
    public class MsSubjectSelectionRuleEnrollment : AuditEntity, IStudentEntity
    {
        public string IdGrade { get; set; }
        public string IdSubject { get; set; }
        public virtual MsGrade Grade { get; set; }
        public virtual MsSubject Subject { get; set; }
    }

    internal class MsSubjectSelectionRuleEnrollmentConfiguration : AuditEntityConfiguration<MsSubjectSelectionRuleEnrollment>
    {
        public override void Configure(EntityTypeBuilder<MsSubjectSelectionRuleEnrollment> builder)
        {
            builder.HasOne(x => x.Grade)
                .WithMany(x => x.SubjectSelectionRuleEnrollments)
                .HasForeignKey(x => x.IdGrade)
                .HasConstraintName("FK_MsSubjectSelectionRuleEnrollment_MsGrade")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder.HasOne(x => x.Subject)
                .WithMany(x => x.SubjectSelectionRuleEnrollments)
                .HasForeignKey(x => x.IdSubject)
                .HasConstraintName("FK_MsSubjectSelectionRuleEnrollment_MsSubject")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            base.Configure(builder);
        }
    }
}