using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Student.FnStudent.Achievement
{
    public class ApprovalAndDeleteAchievementRequest
    {
        public string IdAchievement {  get; set; }
        public AchievementTypeStatus TypeRequest { get; set; }
        public string Note { get; set; }
    }
}
