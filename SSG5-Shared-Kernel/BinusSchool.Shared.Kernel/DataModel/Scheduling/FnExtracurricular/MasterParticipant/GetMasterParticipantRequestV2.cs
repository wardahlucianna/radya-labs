using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnExtracurricular.MasterParticipant
{
    public class GetMasterParticipantRequestV2 : CollectionRequest
    {
        public string IdAcademicYear { get; set; }
        public List<string> IdLevelList { get; set; }
        public List<string> IdGradeList { get; set; }
        public int? Semester { get; set; }
        public string IdExtracurricular { get; set; }
        public bool? Status { get; set; }
    }
}
