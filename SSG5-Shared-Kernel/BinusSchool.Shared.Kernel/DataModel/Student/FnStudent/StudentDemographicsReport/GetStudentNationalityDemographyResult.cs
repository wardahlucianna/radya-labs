using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Student.FnStudent.StudentDemographicsReport
{
    public class GetStudentNationalityDemographyResult
    {
        public int Semester { get; set; }
        public List<string> Header { get; set; }
        public List<Body> Body { get; set; }
        public TotalStudent TotalStudent { get; set; }
    }

    public class Body
    {
        public ItemValueVm Country { get; set; }
        public List<ListData> ListData { get; set; }
        public int Total { get; set; }
    }

    public class TotalStudent
    {
        public List<ListData> ListData { get; set; }
        public int Total { get; set; }
    }

    public class ListData
    {
        public int Male { get; set; }
        public int Female { get; set; }
        public CodeWithIdVm Data { get; set; }
    }
}
