using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Student.FnStudent.Portfolio.LearningGolas
{
    public class AddLearningGoalsRequest
    {
        public string LearningGoals { get; set; }
        public string IdLearningGoalsCategory { get; set; }
        public string IdStudent { get; set; }
        public string IdAcademicYear { get; set; }
        public int Semester { get; set; }
    }
}
