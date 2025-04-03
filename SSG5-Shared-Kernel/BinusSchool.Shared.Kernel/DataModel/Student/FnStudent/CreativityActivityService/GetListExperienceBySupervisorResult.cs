using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Student.FnStudent.CreativityActivityService
{
    public class GetListExperienceBySupervisorResult : ItemValueVm
    {
        public string AcademicYear { get; set; }
        public string ExperienceName { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string Location { get; set; }
        public string Status { get; set; }
    }
}
