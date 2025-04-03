using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnAscTimetable.AscTimeTables
{
    public class AscTimeTableReUploadXmlRequest
    {
        public string IdAscTimeTable { get; set; }
        public string FormatIdClass { get; set; }
        public bool AutomaticGenerateClassId { get; set; }
        public string IdSchool { get; set; }
        public List<string> IdGradePathway { get; set; }
    }
}
