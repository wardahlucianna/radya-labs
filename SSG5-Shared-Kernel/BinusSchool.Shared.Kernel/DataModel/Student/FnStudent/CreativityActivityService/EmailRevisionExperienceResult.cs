using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Student.FnStudent.CreativityActivityService
{
    public class EmailRevisionExperienceResult
    {
        public string Id { get; set; }
        public string StudentName { get; set; }
        public string BinusianId { get; set; }
        public string ExperienceName { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string Location { get; set; }
        public string Status { get; set; }
        public List<string> IdUserCASList { get; set; }
        public string IdUserCas { get; set; }
        public string CasAdvisorName { get; set; }
        public string LastUpdate { get; set; }
    }
}
