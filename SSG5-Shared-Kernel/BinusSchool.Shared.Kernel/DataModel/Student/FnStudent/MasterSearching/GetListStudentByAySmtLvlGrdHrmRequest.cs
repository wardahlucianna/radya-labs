using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Student.FnStudent.MasterSearching
{
    public class GetListStudentByAySmtLvlGrdHrmRequest : CollectionRequest
    {
        public string IdAcademicYear { set; get; }
        public int Semester { set; get; }
        public string IdLevel { set; get; }
        public string IdGrade { set; get; }
        public string IdHomeroom { set; get; }
    }
}
