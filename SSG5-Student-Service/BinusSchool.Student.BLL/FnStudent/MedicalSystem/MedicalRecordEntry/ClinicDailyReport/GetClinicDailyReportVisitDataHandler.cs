using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Student.FnStudent.MedicalSystem.MedicalRecordEntry.ClinicDailyReport;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Persistence.StudentDb.Entities.Employee;

namespace BinusSchool.Student.FnStudent.MedicalSystem.MedicalRecordEntry.ClinicDailyReport
{
    public class GetClinicDailyReportVisitDataHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _context;

        public GetClinicDailyReportVisitDataHandler(IStudentDbContext context)
        {
            _context = context;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var request = Request.ValidateParams<ClinicDailyReportDataRequest>
                (nameof(ClinicDailyReportDataRequest.IdSchool),
                 nameof(ClinicDailyReportDataRequest.Date));

            var response = await GetClinicDailyReportVisitData(request);

            return Request.CreateApiResult2(response as object);
        }

        public async Task<GetClinicDailyReportVisitDataResponse> GetClinicDailyReportVisitData(ClinicDailyReportDataRequest request)
        {
            var response = new GetClinicDailyReportVisitDataResponse();

            #region Visit Data
            var getMedicalRecordVisit = _context.Entity<TrMedicalRecordEntry>()
                .Where(a => a.IdSchool == request.IdSchool
                    && a.CheckInDateTime.Date == request.Date.Date)
                .GroupBy(a => a.IdUser)
                .Select(a => new
                {
                    IdUser = a.Key,
                    VisitCount = a.Count()
                })
                .ToList();

            int studentVisit = _context.Entity<MsStudent>()
                .AsEnumerable()
                .Where(a => getMedicalRecordVisit.Any(b => b.IdUser == a.Id))
                .Select(a => getMedicalRecordVisit.First(b => b.IdUser == a.Id).VisitCount)
                .Sum();

            int staffVisit = _context.Entity<MsStaff>()
                .AsEnumerable()
                .Where(a => getMedicalRecordVisit.Any(b => b.IdUser == a.IdBinusian))
                .Select(a => getMedicalRecordVisit.First(b => b.IdUser == a.IdBinusian).VisitCount)
                .Sum();

            int otherPatientVisit = _context.Entity<MsMedicalOtherUsers>()
                .AsEnumerable()
                .Where(a => getMedicalRecordVisit.Any(b => b.IdUser == a.Id))
                .Select(a => getMedicalRecordVisit.First(b => b.IdUser == a.Id).VisitCount)
                .Sum();

            int totalVisit = studentVisit + staffVisit + otherPatientVisit;

            var totalVisitData = new GetClinicDailyReportVisitDataResponse_Visit()
            {
                Student = studentVisit,
                Staff = staffVisit,
                OtherPatient = otherPatientVisit,
                TotalVisit = totalVisit,
            };
            #endregion

            #region Visitor Data
            var getMedicalRecordVisitor = _context.Entity<TrMedicalRecordEntry>()
                .Where(a => a.IdSchool == request.IdSchool
                    && a.CheckInDateTime.Date == request.Date.Date)
                .ToList();

            int studentVisitor = _context.Entity<MsStudent>()
                .AsEnumerable()
                .Where(a => getMedicalRecordVisitor.Any(b => b.IdUser == a.Id))
                .Select(a => a.Id)
                .Distinct()
                .Count();

            int staffVisitor = _context.Entity<MsStaff>()
                .AsEnumerable()
                .Where(a => getMedicalRecordVisitor.Any(b => b.IdUser == a.IdBinusian))
                .Select(a => a.IdBinusian)
                .Distinct()
                .Count();

            int otherPatientVisitor = _context.Entity<MsMedicalOtherUsers>()
                .AsEnumerable()
                .Where(a => getMedicalRecordVisitor.Any(b => b.IdUser == a.Id))
                .Select(a => a.Id)
                .Distinct()
                .Count();

            int totalVisitor = studentVisitor + staffVisitor + otherPatientVisitor;

            var totalVisitorData = new GetClinicDailyReportVisitDataResponse_Visitor()
            {
                Student = studentVisitor,
                Staff = staffVisitor,
                OtherPatient = otherPatientVisitor,
                TotalVisitor = totalVisitor,
            };
            #endregion

            response.TotalClinicVisit = totalVisitData;
            response.TotalClinicVisitor = totalVisitorData;

            return response;
        }
    }
}
