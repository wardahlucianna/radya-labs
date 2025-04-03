using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Student.FnStudent.Portfolio.LearningGolas
{
    public class UpdateLearningAchievedRequest
    {
        public bool IsAchieved { get; set; }
        public List<UpdateLearningAchieved> LearningAchieveds { get; set; }
    }

    public class UpdateLearningAchieved
    {
        public string Id { get; set; }
    }
}
