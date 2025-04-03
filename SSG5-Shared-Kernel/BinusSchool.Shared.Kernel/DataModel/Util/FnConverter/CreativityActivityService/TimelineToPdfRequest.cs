using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Util.FnConverter.CreativityActivityService
{
    public class TimelineToPdfRequest
    {
        public string IdSchool { get; set; }
        public string IdUser { get; set; }
        public string IdStudent { get; set; }
        public string Role { get; set; }
        public List<string> IdAcademicYear { get; set; }
    }
}
