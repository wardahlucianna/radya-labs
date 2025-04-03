using System.Collections.Generic;

namespace BinusSchool.Data.Model.Student.FnStudent.CreativityActivityService
{
    public class CasExperienceDownloadRequest
    {
        public CasExperienceDownloadRequest()
        {
            IdAcademicYears = new List<string>();
        }

        public string IdUser { get; set; }
        public string IdStudent { get; set; }
        public string IdSchool { get; set; }
        public string Role { get; set; }
        public bool IncludeComment { get; set; }
        public List<string> IdAcademicYears { get; set; }
    }
}
