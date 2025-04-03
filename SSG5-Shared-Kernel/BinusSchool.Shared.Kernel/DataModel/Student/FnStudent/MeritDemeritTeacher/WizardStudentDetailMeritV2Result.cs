using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Student.FnStudent.Achievement;

namespace BinusSchool.Data.Model.Student.FnStudent.MeritDemeritTeacher
{
    public class WizardStudentDetailMeritV2Result
    {
        public string Student { get; set; }
        public string Homeroom { get; set; }
        public int TotalMerit { get; set; }
        public int TotalDemerit { get; set; }
        public string LevelOfInfraction { get; set; }
        public string Sanction { get; set; }
        public List<WizardStudentDetailMeritV2> Merit { get; set; }
    }

    public class WizardStudentDetailMeritV2
    {
        public string Id { get; set; }
        public string AcademicYear { get; set; }
        public int Semester { get; set; }
        public string Achievement { get; set; }
        public string DisciplineName { get; set; }
        public string VerifyingTeacher { get; set; }
        public string FocusArea { get; set; }
        public DateTime? Date { get; set; }
        public DateTime? DateUpdate { get; set; }
        public string Status { get; set; }
        public string ApprovalNote { get; set; }
        public int Point { get; set; }
        public string CreateBy { get; set; }
        public string MeritAchievement { get; set; }
        public AchievementEvidance Evidance {  get; set; }
        public bool IsDisabledDelete {  get; set; }
        public bool IsDisabledEdit { get; set; }
    }
}
