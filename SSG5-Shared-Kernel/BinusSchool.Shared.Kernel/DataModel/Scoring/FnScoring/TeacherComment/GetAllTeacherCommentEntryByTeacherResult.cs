using BinusSchool.Common.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinusSchool.Shared.Kernel.DataModel.Scoring.FnScoring.TeacherComment
{
    public class GetAllTeacherCommentEntryByTeacherResult
    {
        public ItemValueVm AcademicYear { get; set; }
        public string IdStudent { get; set; }
        public ItemValueVm Grade { get; set; }
        public ItemValueVm Period { get; set; }
        public string Comment { get; set; }
        public ItemValueVm HomeroomClass { get; set; }
        public DateTime? EndDate { get; set; }
        public ItemValueVm Subject { get; set; }
        public string ClassroomCode { get; set; }
        public int GradeOrderNumber { get; set; }
    }
    public class GetAllTeacherCommentEntryByTeacherResult_StudentVM
    {
        public string IdAcademicYear { get; set; }
        public string AcademicYearDescription { get; set; }
        public string ClassroomCode { get; set; }
        public string IdGrade { get; set; }
        public string GradeDescription { get; set; }
        public string IdStudent { get; set; }
        public string IdHomeroom { get; set; }
        public string HomeroomDescription { get; set; }
        public int Semester { get; set; }
        public string IdLesson { get; set; }
        public string ClassIdGenerated { get; set; }
        public string IdPeriod { get; set; }
        public string PeriodDescription { get; set; }
        public string IdSubject { get; set; }
        public string SubjectDescription { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int GradeOrderNumber { get; set; }
    }
}
