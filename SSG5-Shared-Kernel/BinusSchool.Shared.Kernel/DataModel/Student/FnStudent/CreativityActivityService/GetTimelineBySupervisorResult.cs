using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Student.FnStudent.CreativityActivityService
{
    public class GetTimelineBySupervisorResult
    {
        public List<GetHeader> Headers { get; set; }
        public List<GetBody> Bodys { get; set; }
        public string Student { get; set; }
    }

    public class GetHeader
    {
        public int Year { get; set; }
        public string Month { get; set; }
        public DateTime Date { get; set; }
    }

    public class GetBody
    {
        public string NameExperience { get; set; }
        public List<GetTimeline> Timeline { get; set; }
    }

    public class GetTimeline
    {
        public int Year { get; set; }
        public string Month { get; set; }
        public bool IsChecked { get; set; }
    }
}
