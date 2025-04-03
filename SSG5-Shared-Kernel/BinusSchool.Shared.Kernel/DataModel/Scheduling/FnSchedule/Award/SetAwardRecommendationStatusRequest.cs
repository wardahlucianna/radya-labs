using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.Award
{
    public class SetAwardRecommendationStatusRequest
    {
        public string Id { get; set; }
        public string IdSchool { get; set; }
        public bool IsSetRecommendation { get; set; }

    }
}