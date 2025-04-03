using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Scoring.FnScoring.LearningContinuum.LearningContinuumEntry
{
    public class GetListStudentLearningContinuumEntryResult
    {
        public GetListStudentLearningContinuumEntryResult_Student Student { get; set; }
        public List<GetListStudentLearningContinuumEntryResult_Summary> Summary { get; set; }
    }

    public class GetListStudentLearningContinuumEntryResult_Student : ItemValueVm
    {
        public Gender Gender { get; set; }
    }

    public class GetListStudentLearningContinuumEntryResult_Summary
    {
        public string IdSubject { get; set; }
        public string SubjectDescription { get; set; }
        public int LOCAchieved { get; set; }
        public int LOCNotAchieved { get; set; } 
    }

    public class GetListStudentLearningContinuumEntryResult_StudentList
    {
        public string StudentName { get; set; }
        public string IdStudent { get; set; }
        public string IdLevel { get; set; }
        public string IdGrade { get; set; }
        public string IdHomeroom { get; set; }
        public string IdLesson { get; set; }
        public string ClassIdGenerated { get; set; }
        public string SubjectID { get; set; }
        public string IdSubject { get; set; }
        public string SubjectName { get; set; }
        public string SubjectShortName { get; set; }
        public int Semester { get; set; }
        public Gender? Gender { get; set; }
    }
}
