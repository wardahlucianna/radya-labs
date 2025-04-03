using System;
using System.Collections.Generic;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.Session
{
    public class GetSessionRequest : CollectionSchoolRequest
    {
        public string IdAcadyear { get; set; }
        public string IdGrade { get; set; }
        public string IdPathway { get; set; }
        public string IdDay { get; set; }
        public string IdSessionSet { get; set; }
    }
}
