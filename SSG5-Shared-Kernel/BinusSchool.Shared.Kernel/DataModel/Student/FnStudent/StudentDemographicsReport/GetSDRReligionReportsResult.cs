using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Student.FnStudent.StudentDemographicsReport
{
    public class GetSDRReligionReportsResult
    {
        public string ViewCategoryType { get; set; }
        public int Semester { get; set; }
        public List<string> Header { get; set; }
        public List<ReligionBody> Body { get; set; }
        public ReligionTotalStudent TotalStudent { get; set; }
    }

    public class ReligionBody
    {
        public CodeWithIdVm CategoryType { get; set; }
        public List<ReligionDataList> DataList { get; set; }
        public int Total { get; set; }
    }

    public class ReligionTotalStudent
    {
        public List<ReligionDataList> DataList { get; set; }
        public int Total { get; set; }
    }

    public class ReligionDataList
    {
        public int Male { get; set; }
        public int Female { get; set; }
        public ItemValueVm Religion { get; set; }
    }
}
