using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Teaching.FnLessonPlan.LessonPlan
{
    public class GetSubjectLessonPlanApprovalResult : ItemValueVm
    {
        public int OrderNumber { get; set; }
        public List<GetGradeLessonPlanApproval> Grade { get; set; }
    }


    public class GetGradeLessonPlanApproval : ItemValueVm
    {
        public List<ItemValueVm> Subject { get; set; }
    }

    
}
