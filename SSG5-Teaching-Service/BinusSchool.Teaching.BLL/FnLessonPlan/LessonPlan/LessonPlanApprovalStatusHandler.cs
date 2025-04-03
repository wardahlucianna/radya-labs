using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Teaching.FnLessonPlan.LessonPlan;
using System.Linq;
using System.Threading.Tasks;

namespace BinusSchool.Teaching.FnLessonPlan.LessonPlan
{
    public class LessonPlanApprovalStatusHandler : FunctionsHttpSingleHandler
    {
        protected override Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.GetParams<GetLessonPlanApprovalStatusRequest>();

            var statuses = new GetLessonPlanApprovalStatusResult[]
            {
                new GetLessonPlanApprovalStatusResult { Description = "Approved" },
                new GetLessonPlanApprovalStatusResult { Description = "Created" },
                new GetLessonPlanApprovalStatusResult { Description = "Need Approval" },
                new GetLessonPlanApprovalStatusResult { Description = "Need Revision" },
                new GetLessonPlanApprovalStatusResult { Description = "Unsubmitted" },
                new GetLessonPlanApprovalStatusResult { Description = "Draft" },
            }.ToList();

            if (!string.IsNullOrEmpty(param.Search))
                statuses = statuses.Where(x => x.Description.Contains(param.Search)).ToList();

            return Task.FromResult(Request.CreateApiResult2(statuses as object));
        }
    }
}