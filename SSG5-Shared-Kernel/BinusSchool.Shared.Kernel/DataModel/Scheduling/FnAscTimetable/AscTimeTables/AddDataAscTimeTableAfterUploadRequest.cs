using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Scheduling.FnAscTimetable.AscTimeTables.UploadXmlModel;

namespace BinusSchool.Data.Model.Scheduling.FnAscTimetable.AscTimeTables
{
    public class AddDataAscTimeTableAfterUploadRequest : UploadXmlFileResult
    {
       public bool SaveLesson { get; set; }
       public bool SaveHomeroom { get; set; }
       public bool SaveHomeroomStudent { get; set; }
       public bool SaveStudentEnrolemnt { get; set; }
       public bool SaveSchedule { get; set; }
    }

}
