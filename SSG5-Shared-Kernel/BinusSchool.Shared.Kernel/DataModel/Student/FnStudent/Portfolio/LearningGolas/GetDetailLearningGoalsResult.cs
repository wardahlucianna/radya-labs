using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Student.FnStudent.Portfolio.LearningGolas
{
    public class GetDetailLearningGoalsResult
    {
        public string Id { get; set; }
        public string LearningGoals { get; set; }
        public CodeWithIdVm LearningGoalsCategory { get; set; }
        public bool IsIbLearningProfile { get; set; }
        public bool IsAppoachesToLearning { get; set; }
        public ItemValueVm AcademicYear { get; set; }
        public int Semester { get; set; }
    }
}
