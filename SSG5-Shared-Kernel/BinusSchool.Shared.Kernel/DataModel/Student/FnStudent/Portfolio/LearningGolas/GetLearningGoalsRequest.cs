using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Student.FnStudent.Portfolio.LearningGolas
{
    public class GetLearningGoalsRequest : CollectionSchoolRequest
    {
        public string IdAcademicYear { get; set; }
        public string IdStudent { get; set; }
        public string IdUser { get; set; }
        public string Role { get; set; }
        public string IdHomeroom { get; set; }
        public string IdLevel { get; set; }
        public string IdGrade { get; set; }
        public int? Semester { get; set; }
    }
}
