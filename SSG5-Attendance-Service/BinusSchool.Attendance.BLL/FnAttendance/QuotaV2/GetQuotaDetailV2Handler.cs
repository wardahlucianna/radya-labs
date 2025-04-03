using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Attendance.FnAttendance.QuotaV2;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities;
using BinusSchool.Persistence.AttendanceDb.Entities.School;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Attendance.FnAttendance.QuotaV2
{
    public class GetQuotaV2DetailHandler : FunctionsHttpSingleHandler
    {
        private readonly IAttendanceDbContext _dbContext;

        public GetQuotaV2DetailHandler(IAttendanceDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            if (!KeyValues.TryGetValue("idLevel", out var idLevel))
                throw new ArgumentNullException(nameof(idLevel));

            var result = new QuotaV2Result();

            var Quota = await _dbContext.Entity<MsQuota>()
                                        .Include(x => x.Level)
                                        .Include(x => x.AcademicYear)
                                        .SingleOrDefaultAsync(x => x.IdLevel == (string)idLevel);

            if (Quota is null)
            {
                var level = await _dbContext.Entity<MsLevel>()
                                        .Include(x => x.AcademicYear)
                                        .SingleOrDefaultAsync(x => x.Id == (string)idLevel);

                if (level is null)
                    throw new NotFoundException("Level is not found");

                result = new QuotaV2Result
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
                    QuotaDetails = new List<QuotaDetailVm>()
                };

                return Request.CreateApiResult2(result as object);
            }

            if (Quota.Level is null)
                throw new NotFoundException("Level is not found");

            var QuotaDetails = await _dbContext.Entity <MsQuotaDetail>()
                                       .Where(x => x.IdQuota == Quota.Id)
                                       .ToListAsync();

            result = new QuotaV2Result
            {
                AcademicYear = new CodeWithIdVm
                {
                    Id = Quota.AcademicYear.Id,
                    Code = Quota.AcademicYear.Code,
                    Description = Quota.AcademicYear.Description
                },
                Level = new CodeWithIdVm
                {
                    Id = Quota.Level.Id,
                    Code = Quota.Level.Code,
                    Description = Quota.Level.Description
                },
                QuotaDetails = new List<QuotaDetailVm>()
            };

            foreach(var item in QuotaDetails)
            {
                var dataDetail = new QuotaDetailVm()
                {
                    AttendanceCategory = item.AttendanceCategory,
                    AbsenceCategory = item.AbsenceCategory,
                    ExcusedAbsenceCategory = item.ExcusedAbsenceCategory,
                    Status = item.Status,
                    Percentage = item.Percentage
                };

                result.QuotaDetails.Add(dataDetail);
            }

            return Request.CreateApiResult2(result as object);
        }
    }
}
