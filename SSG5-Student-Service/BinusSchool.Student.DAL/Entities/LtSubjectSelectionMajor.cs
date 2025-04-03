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
    public class LtSubjectSelectionMajor : AuditEntity, IStudentEntity
    {
        public string IdSchool { get; set; }
        public string MajorName { get; set; }
        public virtual MsSchool School { get; set; }
        public virtual ICollection<MsMappingMajorGrade> MappingMajorGrades { get; set; }
    }

    internal class LtSubjectSelectionMajorConfiguration : AuditEntityConfiguration<LtSubjectSelectionMajor>
    {
        public override void Configure(EntityTypeBuilder<LtSubjectSelectionMajor> builder)
        {
            builder.HasOne(x => x.School)
                .WithMany(x => x.SubjectSelectionMajors)
                .HasForeignKey(x => x.IdSchool)
                .HasConstraintName("FK_LtSubjectSelectionMajor_MsSchool")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            base.Configure(builder);
        }
    }
}