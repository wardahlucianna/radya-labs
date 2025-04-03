using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinusSchool.Student.DAL.Entities
{
    public class TrStudentSubjectSelectionInitial : AuditEntity, IStudentEntity
    {
        public string IdMappingCurriculumSubjectGroupDtl { get; set; }
        public string IdAcademicYear { get; set; }
        public string IdStudent { get; set; }
        public string? IdSubjectSelectionPeriod { get; set; }
        public string IdCurrentGrade { get; set; }
        public int? SelectionPriorityNo { get; set; }
        public string IdUserAction { get; set; }
    }

    internal class TrStudentSubjectSelectionInitialConfiguration : AuditEntityConfiguration<TrStudentSubjectSelectionInitial>
    {
        public override void Configure(EntityTypeBuilder<TrStudentSubjectSelectionInitial> builder)
        {
            base.Configure(builder);
        }
    }
}