using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Data.Model.Student.FnStudent.StudentDemographicsReport;

namespace BinusSchool.Student.FnStudent.StudentDemographicsReport
{
    public class GetStudentDemographicsReportTotalStudentDetailsHandler : FunctionsHttpSingleHandler
    {
        private readonly StudentDemographicsReportHandler _studentDemographicsReportHandler;

        public GetStudentDemographicsReportTotalStudentDetailsHandler(
            StudentDemographicsReportHandler studentDemographicsReportHandler)
        {
            _studentDemographicsReportHandler = studentDemographicsReportHandler;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var columns = new[] { "Student", "Level", "Homeroom", "HomeroomTeacher", "Streaming", "JoinToSchoolDate" };

            var param = await Request.GetBody<GetSDRTotalStudentReportDetailsRequest>();

            var getAllStudentCounterList = await _studentDemographicsReportHandler.StudentDemographicsReport(new StudentDemographicsReportRequest
            {
                IdAcademicYear = param.IdAcademicYear,
                Semester = param.Semester,
                ViewCategoryType = param.ViewCategoryType,
                TotalStudentDetail = true,
                Grade = param.Grade,
                Homeroom = param.Homeroom,
                IdType = param.IdType,
            });

            var result = getAllStudentCounterList.SDRTotalStudentReportDetails;

            if (!string.IsNullOrEmpty(param.Search))
            {
                result = result.Where(x =>
                     (x.Student?.Description ?? "").IndexOf(param.Search, StringComparison.OrdinalIgnoreCase) >= 0 ||
                     (x.Level?.Description ?? "").IndexOf(param.Search, StringComparison.OrdinalIgnoreCase) >= 0 ||
                     (x.Homeroom?.Description ?? "").IndexOf(param.Search, StringComparison.OrdinalIgnoreCase) >= 0 ||
                     (x.HomeroomTeacher?.Description ?? "").IndexOf(param.Search, StringComparison.OrdinalIgnoreCase) >= 0 ||
                     (x.Streaming?.Description ?? "").IndexOf(param.Search, StringComparison.OrdinalIgnoreCase) >= 0
                 ).ToList();
            }

            result = param.OrderBy switch
            {
                "student" => param.OrderType == OrderType.Asc
                            ? result.OrderBy(x => x.Student.Description).ToList()
                            : result.OrderByDescending(x => x.Student.Description).ToList(),
                "level" => param.OrderType == OrderType.Asc
                            ? result.OrderBy(x => x.Level.Description).ToList()
                            : result.OrderByDescending(x => x.Level.Description).ToList(),
                "homeroom" => param.OrderType == OrderType.Asc
                            ? result.OrderBy(x => x.Homeroom.Description).ToList()
                            : result.OrderByDescending(x => x.Homeroom.Description).ToList(),
                "homeroomTeacher" => param.OrderType == OrderType.Asc
                            ? result.OrderBy(x => x.HomeroomTeacher.Description).ToList()
                            : result.OrderByDescending(x => x.HomeroomTeacher.Description).ToList(),
                "streaming" => param.OrderType == OrderType.Asc
                            ? result.OrderBy(x => x.Streaming.Description).ToList()
                            : result.OrderByDescending(x => x.Streaming.Description).ToList(),
                _ => result
            };

            if (param.GetAll == true)
            {
                return Request.CreateApiResult2(result as object);
            }
            else
            {
                var resultPagination = result.SetPagination(param).ToList();

                var count = param.CanCountWithoutFetchDb(resultPagination.Count)
                    ? resultPagination.Count
                    : result.Select(x => x.Student.Id).Count();

                return Request.CreateApiResult2(resultPagination as object, param.CreatePaginationProperty(count).AddColumnProperty(columns));
            }
        }
    }
}
