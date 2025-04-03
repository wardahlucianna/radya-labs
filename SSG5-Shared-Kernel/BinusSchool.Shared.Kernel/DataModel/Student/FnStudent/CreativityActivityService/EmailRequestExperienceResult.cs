using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Student.FnStudent.CreativityActivityService
{
    public class EmailRequestExperienceResult
    {
        public string Id { get; set; }
        public string StudentName { get; set; }
        public string BinusianId { get; set; }
        public string ExperienceName { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string Location { get; set; }
        public string Status { get; set; }
        public List<string> IdUserCAS { get; set; }
    }
}
