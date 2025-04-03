using BinusSchool.Common.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinusSchool.Shared.Kernel.DataModel.Scoring.FnScoring.StudentReflection
{
    public class GetReflectionListResult
    {
        public string IdStudentReflectionPeriod { set; get; }
        public int? MinCharacter { get; set; }
        public int? MaxCharacter { set; get; }
        public string Header { set; get; }
        public string Description { set; get; }
        public CodeWithIdVm AcademicYear { set; get; }
        public ItemValueVm Semester { set; get; }
        public ItemValueVm Term { set; get; }
        public List<GetReflectionListResult_ReflectionData> ReflectionList { set; get; }
        public string FileNameExcel { get; set; }

        public DateTime StartExecuteDate { get; set; }
    }

    public class GetReflectionListResult_ReflectionData
    {
        public NameValueVm Student { set; get; }
        public string? IdStudentReflection { set; get; }
        public string? Reflection { set; get; }
        public DateTime? LastUpdated { set; get; }
        public bool? Status { set; get; }
    }

    public class GetReflectionListResult_HomeroomStudent
    {
        public string IdAcademicYear { get; set; }
        public string AcademicYearCode { get; set; }
        public string AcademicYearDesc { get; set; }
        public string IdLevel { get; set; }
        public string LevelDesc { get; set; }
        public string IdGrade { get; set; }
        public string GradeDesc { get; set; }
        public string IdHomeroom { get; set; }
        public string HomeroomDesc { get; set; }
        public string IdStudent { set; get; }
        public string StudentName { set; get; }
    }
}
