using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnAscTimetable.AscTimeTables
{
    public class AscTimetableGetListRequest : CollectionRequest
    {
        public string IdSchool { get; set; }
        public string IdSessionSet { get; set; }
        public string IdSchoolAcademicyears { get; set; }
        public string IdSchoolLevel { get; set; }
        public string IdSchoolGrade { get; set; }
    }
}
