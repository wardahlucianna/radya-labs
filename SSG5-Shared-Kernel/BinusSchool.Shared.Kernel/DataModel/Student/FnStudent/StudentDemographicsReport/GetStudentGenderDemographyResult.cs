using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Student.FnStudent.StudentDemographicsReport
{
    public class GetStudentGenderDemographyResult
    {
        public int? Semester { get; set; }
        public List<GenderDataList> DataList { get; set; }
        public GenderTotalStudent TotalStudent { get; set; }
    }

    public class GenderDataList
    {
        public ItemValueVm CategoryType { get; set; }
        public int Male { get; set; }
        public int Female { get; set; }
        public int Total { get; set; }
    }

    public class GenderTotalStudent
    {
        public int Male { get; set; }
        public int Female { get; set; }
        public int Total { get; set; }
    }
}
