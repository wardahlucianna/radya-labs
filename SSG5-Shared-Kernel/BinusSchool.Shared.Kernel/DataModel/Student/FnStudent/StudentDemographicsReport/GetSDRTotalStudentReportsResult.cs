using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Student.FnStudent.StudentDemographicsReport
{
    public class GetSDRTotalStudentReportsResult
    {
        public string ViewCategoryType { get; set; }
        public List<GetSDRTotalStudentReportsResult_SemestersData> SemestersData { get; set; }
    }

    public class GetSDRTotalStudentReportsResult_SemestersData
    {
        public string Semester { get; set; }
        public List<GetSDRTotalStudentReportsResult_ListData> ListData { get; set; }
        public GetSDRTotalStudentReportsResult_RowTotal RowTotal { get; set; }
    }

    public class GetSDRTotalStudentReportsResult_ListData : GetSDRTotalStudentReportsResult_RowTotal
    {
        public ItemValueVm CategoryType { get; set; }
    }

    public class GetSDRTotalStudentReportsResult_RowTotal
    {
        public GetSDRTotalStudentReportsResult_IdValue InternalIntake { get; set; }
        public GetSDRTotalStudentReportsResult_IdValue ExternalIntake { get; set; }
        public GetSDRTotalStudentReportsResult_IdValue Inactive { get; set; }
        public GetSDRTotalStudentReportsResult_IdValue WithdrawalProcess { get; set; }
        public GetSDRTotalStudentReportsResult_IdValue Transfer { get; set; }
        public GetSDRTotalStudentReportsResult_IdValue Active { get; set; }
        public GetSDRTotalStudentReportsResult_IdValue Withdrawn { get; set; }
        public GetSDRTotalStudentReportsResult_IdValue TotalStudents { get; set; }
    }

    public class GetSDRTotalStudentReportsResult_IdValue
    {
        public string IdType { get; set; }
        public int Value { get; set; }
    }
}
