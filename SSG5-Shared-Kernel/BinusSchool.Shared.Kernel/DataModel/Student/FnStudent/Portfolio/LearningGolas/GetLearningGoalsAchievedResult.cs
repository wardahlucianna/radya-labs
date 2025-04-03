using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Student.FnStudent.Portfolio.LearningGolas
{
    public class GetLearningGoalsAchievedResult : CodeWithIdVm
    {
        public string LearningGoals { get; set; }
        public CodeWithIdVm LearningGoalsCategory { get; set; }
        public bool Achieved { get; set; }
        public bool IsShowButton { get; set; }
    }
}
