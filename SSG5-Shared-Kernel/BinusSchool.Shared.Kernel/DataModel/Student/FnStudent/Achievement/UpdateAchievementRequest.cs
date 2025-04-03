using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Student.FnStudent.Achievement
{
    public class UpdateAchievementRequest : AddAchievementRequest
    {
        public string Id { get; set; }
        public string IdUser { get; set; }
    }
}
