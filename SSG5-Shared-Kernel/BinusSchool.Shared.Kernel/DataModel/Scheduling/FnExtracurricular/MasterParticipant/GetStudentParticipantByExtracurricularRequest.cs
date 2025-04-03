using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnExtracurricular.MasterParticipant
{
    public class GetStudentParticipantByExtracurricularRequest : CollectionRequest
    {
        public string IdExtracurricular { get; set; }
        public bool? PaymentStatus { get; set; }
    }
}
