using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Student.FnStudent.Achievement
{
    public class AddAchievementRequest
    {
        public string IdHomeroomStudent { get; set; }
        public string AchievementName { get; set; }
        public string IdAchievementCategory { get; set; }
        public string IdFocusArea { get; set; }
        public int Point { get; set; }
        public DateTime Date { get; set; }
        public string IdUserTecaher { get; set; }
        public string IdUser { get; set; }
        public AchievementEvidance Evidance { set; get; }
    }


}
