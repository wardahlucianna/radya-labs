using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Scoring.FnScoring.LearningContinuum.LearningContinuumSummary
{
    public class GetListLearningContinuumSummaryResult
    {
        public GetListLearningContinuumSummaryResult_Student Student { get; set; }
        public List<GetListLearningContinuumSummaryResult_Summary> Summary { get; set; }
        public DateTime? LastSavedDate { get; set; }
        public string LastSavedBy { get; set; }
    }

    public class GetListLearningContinuumSummaryResult_Student : ItemValueVm
    {
        public CodeWithIdVm Grade { get; set; }
        public CodeWithIdVm Classroom { get; set; } 
        public Gender Gender { get; set; }
    }

    public class GetListLearningContinuumSummaryResult_Summary
    {
        public string IdSubject { get; set; }
        public string SubjectDescription { get; set; }
        public int LOCAchieved { get; set; }
        public int LOCNotAchieved { get; set; }
    }

    public class GetListLearningContinuumSummaryResult_StudentList
    {
        public string StudentName { get; set; }
        public string IdStudent { get; set; }
        public string IdLevel { get; set; }
        public string IdGrade { get; set; }
        public string IdHomeroom { get; set; }
        public string GradeDescription { get; set; }
        public string GradeCode { get; set; }
        public string IdClasssroom { get; set; }
        public string ClassroomDescription { get; set; }
        public string ClassroomCode { get; set; }
        public string IdLesson { get; set; }
        public string ClassIdGenerated { get; set; }
        public string SubjectID { get; set; }
        public string IdSubject { get; set; }
        public string SubjectName { get; set; }
        public string SubjectShortName { get; set; }
        public int Semester { get; set; }
        public Gender? Gender { get; set; }
        public int GradeOrderNumber { get; set; }
    }

    public class GetListLearningContinuumSummaryResult_LastSaved
    {
        public DateTime? LastSavedDate { get; set; }
        public string LastSavedBy { get; set; }
    }
}
