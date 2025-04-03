using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Student.FnStudent.MapStudentGrade
{
    public class UploadExcelMapStudentGradeResult
    {
        public string IdAcademicYear { get; set; }
        public List<string> FailedStudents { get; set; }
        public List<ListMapStudentGradeResult> SuccessStudents { get; set; }
    }

    public class ListMapStudentGradeResult
    {
        public string IdStudent { get; set; }
        public string StudentName { get; set; }
        public string Gender { get; set; }
        public ItemValueVm Level { get; set; }
        public ItemValueVm Grade { get; set; }
    }
}
