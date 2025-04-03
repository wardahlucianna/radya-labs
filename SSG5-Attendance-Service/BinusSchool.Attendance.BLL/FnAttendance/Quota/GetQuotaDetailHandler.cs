using System;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Attendance.FnAttendance.Quota;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities;
using BinusSchool.Persistence.AttendanceDb.Entities.School;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Attendance.FnAttendance.Quota
{
    public class GetQuotaDetailHandler : FunctionsHttpSingleHandler
    {
        private readonly IAttendanceDbContext _dbContext;

        public GetQuotaDetailHandler(IAttendanceDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            if (!KeyValues.TryGetValue("idLevel", out var idLevel))
                throw new ArgumentNullException(nameof(idLevel));

            var level = await _dbContext.Entity<MsLevel>()
                                        .Include(x => x.AcademicYear)
                                        .SingleOrDefaultAsync(x => x.Id == (string)idLevel);
            if (level is null)
                throw new NotFoundException("Level is not found");

            var quotas = await _dbContext.Entity<MsMappingAttendanceQuota>()
                                       .Include(x => x.Attendance)
                                       .Where(x => x.IdLevel == (string)idLevel)
                                       .ToListAsync();

            var result = new QuotaResult
            {
                AcademicYear = new CodeWithIdVm
                {
                    Id = level.AcademicYear.Id,
                    Code = level.AcademicYear.Code,
                    Description = level.AcademicYear.Description
                },
                Level = new CodeWithIdVm
                {
                    Id = level.Id,
                    Code = level.Code,
                    Description = level.Description
                },
                Quotas = quotas.Select(x => new AttendanceQuotaVm
                {
                    Attendance = new AttendanceVm
                    {
                        Id = x.Attendance.Id,
                        Code = x.Attendance.Code,
                        Description = x.Attendance.Description,
                        AbsenceCategory = x.Attendance.AbsenceCategory,
                        ExcusedAbsenceCategory = x.Attendance.ExcusedAbsenceCategory
                    },
                    Percentage = x.Percentage
                }).ToList()
            };
            return Request.CreateApiResult2(result as object);
        }
    }
}
