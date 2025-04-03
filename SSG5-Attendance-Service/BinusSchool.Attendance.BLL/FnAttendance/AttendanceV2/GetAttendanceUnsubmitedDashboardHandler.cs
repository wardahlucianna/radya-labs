using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Attendance.FnAttendance.AttendanceV2.Validator;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Data.Api.Attendance.FnAttendance;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceSummaryTerm;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceV2;
using BinusSchool.Persistence.AttendanceDb.Abstractions;

namespace BinusSchool.Attendance.FnAttendance.AttendanceV2
{
    public class GetAttendanceUnsubmitedDashboardHandler : FunctionsHttpSingleHandler
    {
        private readonly IAttendanceDbContext _dbContext;
        private readonly IAttendanceV2 _apiAttendanceV2;

        public GetAttendanceUnsubmitedDashboardHandler(IAttendanceDbContext dbContext, IAttendanceV2 ApiAttendanceV2)
        {
            _dbContext = dbContext;
            _apiAttendanceV2 = ApiAttendanceV2;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<GetAttendanceUnsubmitedDashboardRequest, GetAttendanceUnsubmitedDashboardValidator>();
            string[] _columns = { "date", "clasId", "homeroom", "subject", "session", "totalstudent" };

            List<UnresolvedAttendanceGroupV2Result> listAttendanceUnsubmited = new List<UnresolvedAttendanceGroupV2Result>();
            foreach (var positon in body.SelectedPosition)
            {
                var param = new GetUnresolvedAttendanceV2Request
                {
                    IdAcademicYear = body.IdAcademicYear,
                    IdUser = body.IdUser,
                    CurrentPosition = positon,
                };

                var getUnsubmittedAttendanceFromApi = _apiAttendanceV2.GetUnsubmittedAttendanceV2(param).Result.Payload;
                var getUnsubmittedAttendance = getUnsubmittedAttendanceFromApi.Attendances;

                if(getUnsubmittedAttendance!=null)
                    listAttendanceUnsubmited.AddRange(getUnsubmittedAttendance);
            }

            //var queryUnsubmited = listAttendanceUnsubmited.Distinct();

            //switch (body.OrderBy)
            //{
            //    case "date":
            //        queryUnsubmited = body.OrderType == OrderType.Desc
            //            ? queryUnsubmited.OrderByDescending(x => x.Date)
            //            : queryUnsubmited.OrderBy(x => x.Date);
            //        break;
            //    case "clasId":
            //        queryUnsubmited = body.OrderType == OrderType.Desc
            //            ? queryUnsubmited.OrderByDescending(x => x.ClassID)
            //            : queryUnsubmited.OrderBy(x => x.ClassID);
            //        break;
            //    case "homeroom":
            //        queryUnsubmited = body.OrderType == OrderType.Desc
            //            ? queryUnsubmited.OrderByDescending(x => x.Homeroom.Description)
            //            : queryUnsubmited.OrderBy(x => x.Homeroom.Description);
            //        break;
            //    case "session":
            //        queryUnsubmited = body.OrderType == OrderType.Desc
            //            ? queryUnsubmited.OrderByDescending(x => x.Session)
            //            : queryUnsubmited.OrderBy(x => x.Session);
            //        break;
            //    case "subject":
            //        queryUnsubmited = body.OrderType == OrderType.Desc
            //            ? queryUnsubmited.OrderByDescending(x => x.Subject)
            //            : queryUnsubmited.OrderBy(x => x.Subject);
            //        break;
            //    case "totalstudent":
            //        queryUnsubmited = body.OrderType == OrderType.Desc
            //            ? queryUnsubmited.OrderByDescending(x => x.TotalStudent)
            //            : queryUnsubmited.OrderBy(x => x.TotalStudent);
            //        break;
            //};

            //if (body.Return == CollectionType.Lov)
            //{
            //    listAttendanceUnsubmited = queryUnsubmited
            //        .ToList();
            //}
            //else
            //{
            //    listAttendanceUnsubmited = queryUnsubmited
            //        .SetPagination(body)
            //         .ToList();
            //}

            //var count = body.CanCountWithoutFetchDb(listAttendanceUnsubmited.Count)
            //    ? listAttendanceUnsubmited.Count
            //    : queryUnsubmited.Select(x => x.Date).Count();

            var item = new GetAttendanceUnsubmitedDashboardResult
            {
                IsShowingPopup = false,
                countUnsubmited = listAttendanceUnsubmited.Count(),
                Attendances = listAttendanceUnsubmited
            };

            return Request.CreateApiResult2(item as object);
        }


    }
}
