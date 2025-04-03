using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities.School;
using BinusSchool.Persistence.Entities;
using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Student.DAL.Entities
{
    public class MsSubjectSelectionNextGrade : AuditEntity, IStudentEntity
    {
        public string IdSchool { get; set; }
        public string CurrentCodeGrade { get; set; }
        public string NextCodeGrade { get; set; }
        public virtual MsSchool School { get; set; }
    }

    internal class MsSubjectSelectionNextGradeConfiguration : AuditEntityConfiguration<MsSubjectSelectionNextGrade>
    {
        public override void Configure(EntityTypeBuilder<MsSubjectSelectionNextGrade> builder)
        {
            builder.HasOne(x => x.School)
                .WithMany(x => x.SubjectSelectionNextGrades)
                .HasForeignKey(x => x.IdSchool)
                .HasConstraintName("FK_MsSubjectSelectionNextGrade_MsSchool")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            base.Configure(builder);
        }
    }
}