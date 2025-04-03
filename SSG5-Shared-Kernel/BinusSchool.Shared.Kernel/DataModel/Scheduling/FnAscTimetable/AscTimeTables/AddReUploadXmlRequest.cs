using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Scheduling.FnAscTimetable.AscTimeTables.UploadXmlModel;

namespace BinusSchool.Data.Model.Scheduling.FnAscTimetable.AscTimeTables
{
    public class AddReUploadXmlRequest : UploadXmlFileResult
    {
        public string IdAscTimeTable { get; set; }
    }
}
