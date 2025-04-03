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
using BinusSchool.Persistence.StudentDb.Entities.Employee;
using BinusSchool.Persistence.StudentDb.Entities.Scheduling;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Student.FnStudent.MedicalSystem.MedicalRecordEntry.ClinicDailyReport
{
    public class GetClinicDailyReportInjuryVisitHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _context;
        private readonly IPeriod _period;

        public GetClinicDailyReportInjuryVisitHandler(IStudentDbContext context, IPeriod period)
        {
            _context = context;
            _period = period;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var request = Request.ValidateParams<ClinicDailyReportDataRequest>
                (nameof(ClinicDailyReportDataRequest.IdSchool),
                 nameof(ClinicDailyReportDataRequest.Date));

            var response = await GetClinicDailyReportInjuryVisit(request);

            return Request.CreateApiResult2(response as object);
        }

        public async Task<GetClinicDailyReportInjuryVisitResponse> GetClinicDailyReportInjuryVisit(ClinicDailyReportDataRequest request)
        {
            var incident = new List<GetClinicDailyReportInjuryVisitResponse_Incident>();

            var activeAYData = await _period.GetCurrenctAcademicYear(new Data.Model.School.FnPeriod.Period.CurrentAcademicYearRequest
            {
                IdSchool = request.IdSchool,
            });

            var activeAY = activeAYData.Payload;

            var getHomeroomStudent = _context.Entity<MsHomeroomStudent>()
                .Include(a => a.Student)
                .Include(a => a.Homeroom.Grade.MsLevel)
                .Include(a => a.Homeroom.MsGradePathwayClassroom.Classroom)
                .Where(a => a.Homeroom.Grade.MsLevel.IdAcademicYear == activeAY.Id
                    && a.Semester == activeAY.Semester
                    && a.Homeroom.Semester == activeAY.Semester)
                .ToList();

            var getStaff = _context.Entity<MsStaff>()
                .Where(a => a.IdSchool == request.IdSchool)
                .ToList();

            var getOtherPatient = _context.Entity<MsMedicalOtherUsers>()
                .Where(a => a.IdSchool == request.IdSchool)
                .ToList();

            #region Visit Data
            var getConditionMedicalRecordVisit = _context.Entity<TrMedicalRecordConditionDetails>()
                .Include(a => a.MedicalCondition)
                .Include(a => a.MedicalRecordEntry)
                .Where(a => a.MedicalCondition.IdSchool == request.IdSchool
                    && a.MedicalRecordEntry.IdSchool == request.IdSchool
                    && a.MedicalRecordEntry.CheckInDateTime.Date == request.Date.Date
                    && a.MedicalCondition.MedicalConditionName.ToLower().Contains("injury"))
                .GroupBy(a => new
                {
                    a.MedicalRecordEntry.IdUser,
                    a.MedicalRecordEntry.CheckInDateTime,
                    a.MedicalRecordEntry.CheckOutDateTime,
                    a.MedicalRecordEntry.Location,
                    a.MedicalRecordEntry.TeacherInCharge,
                    a.MedicalRecordEntry.DetailsNotes
                })
                .Select(a => new
                {
                    IdUser = a.Key.IdUser,
                    VisitCount = a.Count(),
                    CheckInDateTime = a.Key.CheckInDateTime,
                    CheckOutDateTime = a.Key.CheckOutDateTime,
                    Location = a.Key.Location,
                    TeacherInCharge = a.Key.TeacherInCharge,
                    DetailsNotes = a.Key.DetailsNotes
                })
                .ToList();

            int studentVisit = _context.Entity<MsStudent>()
                .AsEnumerable()
                .Where(a => getConditionMedicalRecordVisit.Any(b => b.IdUser == a.Id))
                .Sum(a => getConditionMedicalRecordVisit
                    .Where(b => b.IdUser == a.Id)
                    .Sum(b => b.VisitCount));

            int staffVisit = getStaff
                .AsEnumerable()
                .Where(a => getConditionMedicalRecordVisit.Any(b => b.IdUser == a.IdBinusian))
                .Sum(a => getConditionMedicalRecordVisit
                    .Where(b => b.IdUser == a.IdBinusian)
                    .Sum(b => b.VisitCount));

            int otherPatientVisit = getOtherPatient
                .AsEnumerable()
                .Where(a => getConditionMedicalRecordVisit.Any(b => b.IdUser == a.Id))
                .Sum(a => getConditionMedicalRecordVisit
                    .Where(b => b.IdUser == a.Id)
                    .Sum(b => b.VisitCount));
            #endregion

            #region Visitor Data
            var getConditionMedicalRecordVisitor = _context.Entity<TrMedicalRecordConditionDetails>()
                .Include(a => a.MedicalCondition)
                .Include(a => a.MedicalRecordEntry)
                .Where(a => a.MedicalCondition.IdSchool == request.IdSchool
                    && a.MedicalRecordEntry.IdSchool == request.IdSchool
                    && a.MedicalRecordEntry.CheckInDateTime.Date == request.Date.Date
                    && a.MedicalCondition.MedicalConditionName.ToLower().Contains("injury"))
                .ToList();

            int studentVisitor = _context.Entity<MsStudent>()
                .AsEnumerable()
                .Where(a => getConditionMedicalRecordVisitor.Any(b => b.MedicalRecordEntry.IdUser == a.Id))
                .Count();

            int staffVisitor = getStaff
                .AsEnumerable()
                .Where(a => getConditionMedicalRecordVisitor.Any(b => b.MedicalRecordEntry.IdUser == a.IdBinusian))
                .Count();

            int otherPatientVisitor = getOtherPatient
                .AsEnumerable()
                .Where(a => getConditionMedicalRecordVisitor.Any(b => b.MedicalRecordEntry.IdUser == a.Id))
                .Count();
            #endregion

            #region Student Data
            var joinData = from hs in getHomeroomStudent
                           join mri in getConditionMedicalRecordVisit on hs.IdStudent equals mri.IdUser
                           select new
                           {
                               hs = hs,
                               mri = mri
                           };

            var insertStudent = joinData
                .Select(a => new GetClinicDailyReportInjuryVisitResponse_Incident
                {
                    Time = new GetClinicDailyReportInjuryVisitResponse_Incident.GetClinicDailyReportInjuryVisitResponse_Incident_Time
                    {
                        CheckIn = a.mri.CheckInDateTime.TimeOfDay,
                        CheckOut = a.mri.CheckOutDateTime.HasValue
                        ? a.mri.CheckOutDateTime.Value.TimeOfDay
                        : TimeSpan.Zero,
                    },
                    Name = NameUtil.GenerateFullName(a.hs.Student.FirstName, a.hs.Student.LastName),
                    Grade = new CodeWithIdVm
                    {
                        Id = a.hs.Homeroom.IdGrade,
                        Description = a.hs.Homeroom.Grade.Description + a.hs.Homeroom.MsGradePathwayClassroom.Classroom.Code,
                        Code = a.hs.Homeroom.Grade.Code + a.hs.Homeroom.MsGradePathwayClassroom.Classroom.Code
                    },
                    Location = a.mri.Location ?? "-",
                    Teacher = a.mri.TeacherInCharge ?? "-",
                    Notes = a.mri.DetailsNotes ?? "-"
                })
                .OrderBy(a => a.Name)
                    .ThenBy(a => a.Grade.Code.Length)
                    .ThenBy(a => a.Grade.Code)
                    .ThenBy(a => a.Time.CheckIn)
                .ToList();

            incident.AddRange(insertStudent);
            #endregion

            #region Staff Data
            var staffData = from s in getStaff
                            join mri in getConditionMedicalRecordVisit on s.IdBinusian equals mri.IdUser
                            select new
                            {
                                s = s,
                                mri = mri,
                            };

            var insertStaff = staffData
                .Select(a => new GetClinicDailyReportInjuryVisitResponse_Incident
                {
                    Time = new GetClinicDailyReportInjuryVisitResponse_Incident.GetClinicDailyReportInjuryVisitResponse_Incident_Time
                    {
                        CheckIn = a.mri.CheckInDateTime.TimeOfDay,
                        CheckOut = a.mri.CheckOutDateTime.HasValue
                        ? a.mri.CheckOutDateTime.Value.TimeOfDay
                        : TimeSpan.Zero,
                    },
                    Name = NameUtil.GenerateFullName(a.s.FirstName, a.s.LastName),
                    Grade = new CodeWithIdVm(),
                    Location = a.mri.Location ?? "-",
                    Teacher = a.mri.TeacherInCharge ?? "-",
                    Notes = a.mri.DetailsNotes ?? "-"
                })
                .OrderBy(a => a.Name)
                    .ThenBy(a => a.Time.CheckIn)
                .ToList();

            incident.AddRange(insertStaff);
            #endregion

            #region Other(s) Data
            var otherPatientData = from op in getOtherPatient
                                   join mri in getConditionMedicalRecordVisit on op.Id equals mri.IdUser
                                   select new
                                   {
                                       op = op,
                                       mri = mri,
                                   };

            var insertOtherPatient = otherPatientData
                .Select(a => new GetClinicDailyReportInjuryVisitResponse_Incident
                {
                    Time = new GetClinicDailyReportInjuryVisitResponse_Incident.GetClinicDailyReportInjuryVisitResponse_Incident_Time
                    {
                        CheckIn = a.mri.CheckInDateTime.TimeOfDay,
                        CheckOut = a.mri.CheckOutDateTime.HasValue
                        ? a.mri.CheckOutDateTime.Value.TimeOfDay
                        : TimeSpan.Zero,
                    },
                    Name = a.op.MedicalOtherUsersName,
                    Grade = new CodeWithIdVm(),
                    Location = a.mri.Location ?? "-",
                    Teacher = a.mri.TeacherInCharge ?? "-",
                    Notes = a.mri.DetailsNotes ?? "-"
                })
                .OrderBy(a => a.Name)
                    .ThenBy(a => a.Time.CheckIn)
                .ToList();

            incident.AddRange(insertOtherPatient);
            #endregion

            var response = new GetClinicDailyReportInjuryVisitResponse()
            {
                Summary = new GetClinicDailyReportInjuryVisitResponse_Visit
                {
                    TotalOccurrence = (studentVisit + staffVisit + otherPatientVisit),
                    UniqueIndividual = (studentVisitor + staffVisitor + otherPatientVisitor),
                },
                Incidents = incident
            };

            return response;
        }
    }
}
