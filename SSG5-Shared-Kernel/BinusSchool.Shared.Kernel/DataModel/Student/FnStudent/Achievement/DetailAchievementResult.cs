using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Student.FnStudent.Achievement
{
    public class DetailAchievementResult
    {
        public string Id { get; set; }
        public AchievementStudent Student { get; set; }
        public string AchievementName { get; set; }
        public ItemValueVm AchievementCategory { get; set; }
        public ItemValueVm FocusArea { get; set; }
        public DateTime DateOfCompletion {  get; set; }
        public ItemValueVm UserTeacher { get; set; }
        public AchievementEvidance Evidance { set; get; }
    }

    public class AchievementStudent
    {
        public string IdStudent { get; set; }
        public string StudentName { get; set; }
        public string IdHomeroomStudent { get; set; }
        public ItemValueVm Level { get; set; }
        public ItemValueVm Grade { get; set; }
        public ItemValueVm Homeroom { get; set; }
    }
}
