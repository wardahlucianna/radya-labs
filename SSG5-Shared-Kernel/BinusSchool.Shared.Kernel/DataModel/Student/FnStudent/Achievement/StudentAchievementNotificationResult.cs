using System;

namespace BinusSchool.Data.Model.Student.FnStudent.Achievement
{
    public class StudentAchievementNotificationResult
    {
        public string Id { get; set; }
        public string AcademicYear { get; set; }
        public int Semester { get; set; }
        public string AchievementName { get; set; }
        public string AchievementCategory { get; set; }
        public string FocusArea { get; set; }
        public string StudentName { get; set; }
        public DateTime DateCompletion { get; set; }
        public int Point { get; set; }
        public string ApprovalNotes { get; set; }
        public string Status { get; set; }
        // Approved & Declined by verifying teacher
        public string IdVerifyingTeacher { get; set; }
        public string VerifyingTeacher { get; set; }
        public string CreatedBy { get; set; }
    }
}
