using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Student.FnStudent.StudentDemographicsReport.StudentDemographicsGenerateExcelData
{
    public class StudentDemographicsDetailGenerateExcelDataResult
    {
        public int Semester { get; set; }
        public List<GetStudentGenderDemographyDetailResult> GenderDetail { get; set; }
        public List<DataList> NationalityDetail { get; set; }
        public List<GetSDRReligionReportDetailsResult> ReligionDetail { get; set; }
        public List<GetStudentTotalFamilyDemographicsDetailResult> TotalFamilyDetail { get; set; }
        public List<GetSDRTotalStudentReportDetailsResult> TotalStudentDetail { get; set; }
    }
}
