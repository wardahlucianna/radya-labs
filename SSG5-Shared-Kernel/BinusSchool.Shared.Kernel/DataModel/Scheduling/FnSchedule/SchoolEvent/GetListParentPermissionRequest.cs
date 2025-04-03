using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.SchoolEvent
{
    public class GetListParentPermissionRequest : CollectionSchoolRequest
    {
        public string IdEvent { get; set; }
        public string IdGrade { get; set; }
        public string IdHomeroom { get; set; }
        public string ApprovalStatus { get; set; }
    }
}