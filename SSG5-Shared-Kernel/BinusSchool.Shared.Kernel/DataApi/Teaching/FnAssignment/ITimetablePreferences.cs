using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Teaching.FnAssignment.Timetable;
using Refit;

namespace BinusSchool.Data.Api.Teaching.FnAssignment
{
    public interface ITimetablePreferences : IFnAssignment
    {
        [Post("/assignment/timetable-preferences")]
        Task<ApiErrorResult> AddTimeTable([Body] AddTimeTableRequest body);
        [Post("/assignment/timetable-preferences-add")]
        Task<ApiErrorResult> PostTimeTable([Body] PostTimetableRequest body);

        [Put("/assignment/timetable-preferences")]
        Task<ApiErrorResult> UpdateTimeTable([Body] UpdateTimetableRequest body);

        [Post("/assignment/timetable-preferences/merge-unmerge")]
        Task<ApiErrorResult> MergeUnmerge([Body] AddMergeUnmergeRequest body);

        [Get("/assignment/timetable-preferences")]
        Task<ApiErrorResult<IEnumerable<GetListTimeTableResult>>> GetTimetables(GetListTimeTableRequest query);

        [Post("/assignment/timetable-preferences/status")]
        Task<ApiErrorResult<IEnumerable<TimetableResult>>> GetTimetableStatus(IEnumerable<string> ids);

        [Get("/assignment/timetable-preferences-by-user")]
        Task<ApiErrorResult<IEnumerable<GetTimeTableByUserResult>>> GetTimetableByUser([Query] GetTimeTableByUserRequest query);

        [Get("/assignment/timetable-preferences-export")]
        Task<HttpResponseMessage> ExportTimeTable(ExportExcelTimeTableRequest request);

        [Delete("/assignment/timetable-preferences")]
        Task<ApiErrorResult> DeleteTimeTable([Body] IEnumerable<string> ids);

        [Get("/assignment/timetable-preferences-dashboard")]
        Task<ApiErrorResult<GetDashboardTimeTableResult>> GetTimetableDashboard([Query] GetDashboardTimeTableRequest query);

        [Post("/assignment/timetable-preferences-detail")]
        Task<ApiErrorResult> TimetableDetail([Body] TimetableDetailRequest body);
    }
}
