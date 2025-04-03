using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Student.FnStudent.CreativityActivityService
{
    public class GetTimelineBySupervisorRequest
    {
        public string IdUser { get; set; }
        public string IdStudent { get; set; }
        public string Role { get; set; }
        public bool IsCASCoordinator { get; set; }
        public List<string> IdAcademicYear { get; set; }
    }
}
