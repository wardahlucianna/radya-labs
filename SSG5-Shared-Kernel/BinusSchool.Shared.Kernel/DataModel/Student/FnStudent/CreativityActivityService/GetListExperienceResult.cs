using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Student.FnStudent.CreativityActivityService
{
    public class GetListExperienceResult : CodeWithIdVm
    {
        public string AcademicYear { get; set; }
        public string ExperienceName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Location { get; set; }
        public string Status { get; set; }
        public string StudentName { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
    }
}
