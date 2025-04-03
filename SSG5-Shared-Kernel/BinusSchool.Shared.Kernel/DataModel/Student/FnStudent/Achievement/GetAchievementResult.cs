using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Student.FnStudent.Achievement
{
    public class GetAchievementResult : ItemValueVm
    {
        public string AcademicYear {set; get; }
        public int Semester { set; get; }
        public string AchievementName { set; get; }
        public string AchievementCategory { set; get; }
        public string VerifyingTeacher { set; get; }
        public string FocusArea { set; get; }
        public DateTime DateOfCompletion { set; get; }
        public AchievementEvidance Evidance { set; get; }
        public int Point { set; get; }
        public string CreateBy { set; get; }
        public string Status { set; get; }
        public string ApprovalNote { set; get; }
        public string Type { set; get; }
        public AchievementStudent Student { set; get; }
    }

    public class AchievementEvidance
    {
        public string Id { get; set; }
        public string OriginalName { get; set; }
        public int FileSize { get; set; }
        public string FileName { get; set; }
        public string Url { get; set; }
        public string FileType { get; set; }
    }
}
