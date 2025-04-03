using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnSchedule.SchoolVisitor;
using Refit;

namespace BinusSchool.Data.Api.Scheduling.FnSchedule
{
    public interface ISchoolVisitor : IFnSchedule
    {
        [Get("/schedule/school-visitor")]
        Task<ApiErrorResult<IEnumerable<GetSchoolVisitorResult>>> GetSchoolVisitor(GetSchoolVisitorRequest query);

        [Post("/schedule/school-visitor")]
        Task<ApiErrorResult>AddSchoolVisitor([Body] AddSchoolVisitorRequest body);

        [Get("/schedule/school-visitor/{id}")]
        Task<ApiErrorResult<DetailSchoolVisitorResult>> DetailSchoolVisitor(string id);

        [Put("/schedule/school-visitor")]
        Task<ApiErrorResult> UpdateSchoolVisitor([Body] UpdateSchoolVisitorRequest body);

        [Delete("/schedule/school-visitor")]
        Task<ApiErrorResult> DeleteSchoolVisitor([Body] IEnumerable<string> body);

    }
}
