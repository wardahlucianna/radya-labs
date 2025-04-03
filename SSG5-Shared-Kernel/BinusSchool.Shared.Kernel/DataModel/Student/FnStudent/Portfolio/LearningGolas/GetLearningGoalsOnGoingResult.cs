using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;

namespace BinusSchool.Data.Model.Student.FnStudent.Portfolio.LearningGolas
{
    public class GetLearningGoalsOnGoingResult : CodeWithIdVm
    {
        public string LearningGoals { get; set; }
        public string LearningGoalsCategory { get; set; }
        public string CreateBy { get; set; }
        public bool IsGoing { get; set; }
        public bool IsShowButton { get; set; }
    }
}
