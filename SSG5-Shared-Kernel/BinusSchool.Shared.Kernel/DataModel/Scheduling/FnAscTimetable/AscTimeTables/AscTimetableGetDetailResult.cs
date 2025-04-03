using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Scheduling.FnAscTimetable.AscTimeTables.UploadXmlModel;

namespace BinusSchool.Data.Model.Scheduling.FnAscTimetable.AscTimeTables
{
    public class AscTimetableGetDetailResult : UploadXmlFileResult
    {
        public string IdAscTimeTableName { get; set; }
        public string AscTimeTableName { get; set; }
        public string Acadedmicyears { get; set; }
        public List<ParticipanVm> Participan { get; set; } = new List<ParticipanVm>();
        public string ClassIdExampleValue { get; set; }
    }

    public class ParticipanVm 
    {
        public string Grade { get; set; }

        public string Pathway { get; set; }
        public string IdGradePathway { get; set; }
       // public GradePathwayAsc pathway { get; set; }
       
    }

}

