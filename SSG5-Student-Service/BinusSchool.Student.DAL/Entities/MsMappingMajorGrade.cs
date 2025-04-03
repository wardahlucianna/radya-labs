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
    public class MsMappingMajorGrade : AuditEntity, IStudentEntity
    {
        public string IdSubjectSelectionMajor { get; set; }
        public string IdGrade { get; set; }
        public virtual LtSubjectSelectionMajor SubjectSelectionMajor { get; set; }
        public virtual MsGrade Grade { get; set; }
        public virtual ICollection<TrStudentSubjectSelectionMajor> StudentSubjectSelectionMajors { get; set; }
    }

    internal class MsMappingMajorGradeConfiguration : AuditEntityConfiguration<MsMappingMajorGrade>
    {
        public override void Configure(EntityTypeBuilder<MsMappingMajorGrade> builder)
        {
            builder.HasOne(x => x.SubjectSelectionMajor)
                .WithMany(x => x.MappingMajorGrades)
                .HasForeignKey(x => x.IdSubjectSelectionMajor)
                .HasConstraintName("FK_MsMappingMajorGrade_LtSubjectSelectionMajor")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder.HasOne(x => x.Grade)
                .WithMany(x => x.MappingMajorGrades)
                .HasForeignKey(x => x.IdGrade)
                .HasConstraintName("FK_MsMappingMajorGrade_MsGrade")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            base.Configure(builder);
        }
    }
}