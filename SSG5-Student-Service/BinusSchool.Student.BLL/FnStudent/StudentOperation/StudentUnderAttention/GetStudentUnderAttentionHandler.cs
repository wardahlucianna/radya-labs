using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Api.School.FnPeriod;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Persistence.StudentDb.Entities.Scheduling;
using BinusSchool.Shared.Kernel.DataModel.Student.FnStudent.StudentOperation.StudentUnderAttention;
using BinusSchool.Student.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinusSchool.Student.BLL.FnStudent.StudentOperation.StudentUnderAttention
{
    public class GetStudentUnderAttentionHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _context;
        private readonly IPeriod _period;

        public GetStudentUnderAttentionHandler(IStudentDbContext context, IPeriod period)
        {
            _context = context;
            _period = period;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var request = Request.ValidateParams<GetStudentUnderAttentionRequest>(
                nameof(GetStudentUnderAttentionRequest.IdSchool));

            var response = new List<GetStudentUnderAttentionResponse>();

            var currentPeriod = _period.GetCurrenctAcademicYear(new Data.Model.School.FnPeriod.Period.CurrentAcademicYearRequest
            {
                IdSchool = request.IdSchool,
            });

            var activeAY = currentPeriod.Result.Payload.Code;

            var trStudentStatuses = await _context.Entity<TrStudentStatus>()
                .Include(a => a.AcademicYear.MsSchool)
                .Include(a => a.Student)
                .Include(a => a.StudentStatusSpecial)
                .Where(a => a.AcademicYear.IdSchool == request.IdSchool
                    && (string.IsNullOrWhiteSpace(request.IdAcademicYear) ? true : a.IdAcademicYear == request.IdAcademicYear)
                    && a.StudentStatusSpecial.NeedAttention == true
                    && a.CurrentStatus == "A")
                .ToListAsync(CancellationToken);

            // Step 1: Get the highest semester for each student in MsHomeroomStudent
            var maxSemesterForStudents = _context.Entity<MsHomeroomStudent>()
                .Include(a => a.Homeroom.Grade.MsLevel)
                .Include(a => a.Homeroom.MsGradePathwayClassroom.Classroom)
                .Where(a => (string.IsNullOrWhiteSpace(request.IdAcademicYear) ? true : a.Homeroom.Grade.MsLevel.IdAcademicYear == request.IdAcademicYear)
                    && a.Homeroom.Grade.MsLevel.MsAcademicYear.IdSchool == request.IdSchool)
                .GroupBy(hs => new { hs.IdStudent, hs.Homeroom.Grade.MsLevel.IdAcademicYear })
                .Select(g => g.OrderByDescending(hs => hs.Semester).FirstOrDefault())
                .ToList();

            // Step 2: Join the filtered max semester result with trStudentStatuses
            var joinWithHomeroom = from tss in trStudentStatuses
                                   join hs in maxSemesterForStudents
                                        on new { tss.IdAcademicYear, tss.IdStudent } equals new { hs.Homeroom.Grade.MsLevel.IdAcademicYear, hs.IdStudent } into hs_group
                                   from hsGroup in hs_group.DefaultIfEmpty()
                                   join ad in _context.Entity<MsAdmissionData>()
                                        on tss.IdStudent equals ad.IdStudent into ad_group
                                   from adGroup in ad_group.DefaultIfEmpty()
                                   select new
                                   {
                                       tss = tss,
                                       hs = hsGroup,
                                       ad = adGroup
                                   };

            var data = joinWithHomeroom

                .Select(a => new GetStudentUnderAttentionResponse
                {
                    IdTrStudentStatus = a.tss.IdTrStudentStatus,
                    School = new ItemValueVm
                    {
                        Id = a.tss.AcademicYear.IdSchool,
                        Description = a.tss.AcademicYear.MsSchool.Description,
                    },
                    Student = new ItemValueVm
                    {
                        Id = a.tss.IdStudent,
                        Description = NameUtil.GenerateFullName(a.tss.Student.FirstName,  a.tss.Student.LastName)
                    },
                    LastHomeroom = new ItemValueVm
                    {
                        Id = a.hs?.IdHomeroom ?? "-",
                        Description = string.IsNullOrWhiteSpace($"{a.hs?.Homeroom?.Grade?.Code} {a.hs?.Homeroom?.MsGradePathwayClassroom?.Classroom?.Code}".Trim()) ? "-" : $"{a.hs?.Homeroom?.Grade?.Code} {a.hs?.Homeroom?.MsGradePathwayClassroom?.Classroom?.Code}".Trim()
                    },
                    LastAcademicYear = new ItemValueVm
                    {
                        Id = a.tss.IdAcademicYear,
                        Description = a.tss.AcademicYear.Code
                    },
                    Status = a.tss.StudentStatusSpecial.LongDesc,
                    JoinDate = a.ad?.JoinToSchoolDate.Value.Date ?? null,
                    LastActiveDate = a.tss.StartDate.Date,
                    Remarks = a.tss.Remarks ?? "-",
                    CanEdit = a.tss.AcademicYear.Code != activeAY ? false : a.tss.StudentStatusSpecial.NeedFutureAdmissionDecision
                })
                .Distinct()
                .OrderByDescending(a => a.LastActiveDate)
                    .ThenBy(a => a.Student.Description)
                .ToList();

            if (!string.IsNullOrEmpty(request.Search))
                data = data
                    .Where(a => a.Student.Description.Contains(request.Search, StringComparison.OrdinalIgnoreCase)
                        || a.Student.Id.Contains(request.Search, StringComparison.OrdinalIgnoreCase))
                    .ToList();

            response.AddRange(data);

            response = request.OrderBy switch
            {
                "School" => request.OrderType == Common.Model.Enums.OrderType.Asc
                    ? response.OrderBy(a => a.School.Id).ToList()
                    : response.OrderByDescending(a => a.School.Id).ToList(),
                "Name" => request.OrderType == Common.Model.Enums.OrderType.Asc
                    ? response.OrderBy(a => a.Student.Description).ToList()
                    : response.OrderByDescending(a => a.Student.Description).ToList(),
                "AcademicYear" => request.OrderType == Common.Model.Enums.OrderType.Asc
                    ? response.OrderBy(a => a.LastAcademicYear.Description).ToList()
                    : response.OrderByDescending(a => a.LastAcademicYear.Description).ToList(),
                _ => response.OrderByDescending(a => a.LastAcademicYear.Description).ToList()
            };

            var count = request.CanCountWithoutFetchDb(response.Count)
                ? response.Count
                : response.Select(a => a.IdTrStudentStatus).Count();

            response = response.SetPagination(request).ToList();

            return Request.CreateApiResult2(response as object, request.CreatePaginationProperty(count));
        }
    }
}
