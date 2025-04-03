using System.Collections.Generic;

namespace BinusSchool.Data.Model.Student.FnLongRun
{
    public class ExperienceDownloadRequest
    {
        public ExperienceDownloadRequest()
        {
            IdAcademicYears = new List<string>();
        }

        public string Id { get; set; }
        public string IdUser { get; set; }
        public string IdStudent { get; set; }
        public string IdSchool { get; set; }
        public string Role { get; set; }
        public bool IncludeComment { get; set; }
        public List<string> IdAcademicYears { get; set; }
    }
}
