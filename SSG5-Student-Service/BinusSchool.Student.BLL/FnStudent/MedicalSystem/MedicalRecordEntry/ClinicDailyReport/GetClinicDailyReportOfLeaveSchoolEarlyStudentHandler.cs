using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Api.School.FnPeriod;
using BinusSchool.Data.Model.Student.FnStudent.MedicalSystem.MedicalRecordEntry.ClinicDailyReport;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Persistence.StudentDb.Entities.Scheduling;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Student.FnStudent.MedicalSystem.MedicalRecordEntry.ClinicDailyReport
{
    public class GetClinicDailyReportOfLeaveSchoolEarlyStudentHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _context;
        private readonly IPeriod _period;

        public GetClinicDailyReportOfLeaveSchoolEarlyStudentHandler(IStudentDbContext context, IPeriod period)
        {
            _context = context;
            _period = period;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var request = Request.ValidateParams<ClinicDailyReportDataRequest>
                (nameof(ClinicDailyReportDataRequest.IdSchool),
                 nameof(ClinicDailyReportDataRequest.Date));

            var response = await GetClinicDailyReportOfLeaveSchoolEarlyStudent(request);

            return Request.CreateApiResult2(response as object);
        }

        public async Task<List<GetClinicDailyReportOfLeaveSchoolEarlyStudentResponse>> GetClinicDailyReportOfLeaveSchoolEarlyStudent(ClinicDailyReportDataRequest request)
        {
            var response = new List<GetClinicDailyReportOfLeaveSchoolEarlyStudentResponse>();

            var activeAYData = await _period.GetCurrenctAcademicYear(new Data.Model.School.FnPeriod.Period.CurrentAcademicYearRequest
            {
                IdSchool = request.IdSchool,
            });

            var activeAY = activeAYData.Payload;

            var getHomeroomStudent = _context.Entity<MsHomeroomStudent>()
                .Include(a => a.Homeroom.Grade.MsLevel)
                .Include(a => a.Homeroom.MsGradePathwayClassroom.Classroom)
                .Include(a => a.Student)
                .Where(a => a.Homeroom.Grade.MsLevel.IdAcademicYear == activeAY.Id
                    && a.Semester == activeAY.Semester
                    && a.Homeroom.Semester == activeAY.Semester)
                .ToList();

            var getMedicalRecord = _context.Entity<TrMedicalRecordEntry>()
                .Where(a => a.IdSchool == request.IdSchool
                    && a.CheckInDateTime.Date == request.Date.Date
                    && a.DismissedHome == true)
                .ToList();

            var joinData = from hs in getHomeroomStudent
                           join mr in getMedicalRecord on hs.Student.Id equals mr.IdUser
                           select new
                           {
                               hs = hs,
                               mr = mr
                           };

            var insertStudent = joinData
                .Select(a => new GetClinicDailyReportOfLeaveSchoolEarlyStudentResponse
                {
                    Time = new GetClinicDailyReportOfLeaveSchoolEarlyStudentResponse_Time
                    {
                        CheckIn = a.mr.CheckInDateTime.TimeOfDay,
                        CheckOut = a.mr.CheckOutDateTime.HasValue ? a.mr.CheckOutDateTime.Value.TimeOfDay : TimeSpan.Zero,
                    },
                    Name = NameUtil.GenerateFullName(a.hs.Student.FirstName, a.hs.Student.LastName),
                    Grade = new CodeWithIdVm
                    {
                        Id = a.hs.Homeroom.IdGrade,
                        Description = a.hs.Homeroom.Grade.Description + a.hs.Homeroom.MsGradePathwayClassroom.Classroom.Code,
                        Code = a.hs.Homeroom.Grade.Code + a.hs.Homeroom.MsGradePathwayClassroom.Classroom.Code
                    },
                    Location = a.mr.Location ?? "-",
                    Teacher = a.mr.TeacherInCharge ?? "-",
                    Notes = a.mr.TeacherInCharge ?? "-"
                })
                .OrderBy(a => a.Name)
                    .ThenBy(a => a.Grade.Code.Length)
                    .ThenBy(a => a.Grade.Code)
                    .ThenBy(a => a.Time.CheckIn)
                .ToList();

            response.AddRange(insertStudent);

            return response;
        }
    }
}
