using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Student.FnStudent.StudentDemographicsReport;

namespace BinusSchool.Student.FnStudent.StudentDemographicsReport
{
    public class GetStudentDemographicsReportTotalStudentHandler : FunctionsHttpSingleHandler
    {
        private readonly StudentDemographicsReportHandler _studentDemographicsReportHandler;

        public GetStudentDemographicsReportTotalStudentHandler(StudentDemographicsReportHandler studentDemographicsReportHandler)
        {
            _studentDemographicsReportHandler = studentDemographicsReportHandler;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.GetBody<GetSDRTotalStudentReportsRequest>();

            var getAllStudentCounterList = await _studentDemographicsReportHandler.StudentDemographicsReport(new StudentDemographicsReportRequest
            {
                IdAcademicYear = param.IdAcademicYear,
                Level = param.Level,
                Grade = param.Grade,
                Semester = param.Semester,
                ViewCategoryType = param.ViewCategoryType,
                TotalStudent = true
            });

            return Request.CreateApiResult2(getAllStudentCounterList.SDRTotalStudentReports as object);
        }
    }
}
