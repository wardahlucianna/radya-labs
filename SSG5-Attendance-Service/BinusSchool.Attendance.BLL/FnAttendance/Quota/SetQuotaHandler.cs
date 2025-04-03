using System;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Attendance.FnAttendance.Quota.Validator;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Attendance.FnAttendance.Quota;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities;
using BinusSchool.Persistence.AttendanceDb.Entities.School;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;

namespace BinusSchool.Attendance.FnAttendance.Quota
{
    public class SetQuotaHandler : FunctionsHttpSingleHandler
    {
        protected IDbContextTransaction Transaction;
        private readonly IAttendanceDbContext _dbContext;

        public SetQuotaHandler(IAttendanceDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected async override Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<SetQuotaRequest, SetQuotaValidator>();
            Transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

            if (!await _dbContext.Entity<MsLevel>()
                                 .AnyAsync(x => x.Id == body.IdLevel))
                throw new NotFoundException("Level is not found");

            var attendanceIds = body.Quotas.Select(x => x.IdAttendance).ToList();
            var attendances = await _dbContext.Entity<MsAttendance>()
                                              .Where(x => attendanceIds.Contains(x.Id))
                                              .ToListAsync();

            // throw when any attendance that not found
            var notFoundAttendances = attendanceIds.Except(attendances.Select(x => x.Id));
            if (notFoundAttendances.Any())
                throw new BadRequestException($"Some att is not found: {string.Join(", ", notFoundAttendances)}");

            // make sure no multiple attendance from requested entries
            var duplicateEntries = attendanceIds.GroupBy(x => x)
                                                .Where(x => x.Count() > 1)
                                                .Select(x => x.First());
            if (duplicateEntries.Any())
                throw new BadRequestException($"You entered multiple entry for attendance(s) {string.Join(", ", duplicateEntries)}.");

            var quotas = await _dbContext.Entity<MsMappingAttendanceQuota>()
                                         .Where(x => x.IdLevel == body.IdLevel)
                                         .ToListAsync();

            var deletedQuotas = quotas.Where(x => !attendanceIds.Contains(x.IdAttendance)).ToList();
            if (deletedQuotas.Any())
            {
                foreach (var deleted in deletedQuotas)
                {
                    deleted.IsActive = false;
                    _dbContext.Entity<MsMappingAttendanceQuota>().Update(deleted);
                }
            }

            foreach (var item in body.Quotas)
            {
                var quota = quotas.FirstOrDefault(x => x.IdAttendance == item.IdAttendance);
                if (quota is null)
                    _dbContext.Entity<MsMappingAttendanceQuota>().Add(new MsMappingAttendanceQuota
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdLevel = body.IdLevel,
                        IdAttendance = item.IdAttendance,
                        Percentage = item.Percentage
                    });
                else
                {
                    quota.IdAttendance = item.IdAttendance;
                    quota.Percentage = item.Percentage;
                    _dbContext.Entity<MsMappingAttendanceQuota>().Update(quota);
                }
            }

            await _dbContext.SaveChangesAsync(CancellationToken);
            await Transaction.CommitAsync(CancellationToken);

            return Request.CreateApiResult2();
        }

        protected override Task<IActionResult> OnException(Exception ex)
        {
            Logger.LogError(ex, ex.Message);
            Transaction?.Rollback();
            var response = Request.CreateApiErrorResponse(ex);

            return Task.FromResult(response as IActionResult);
        }

        protected override void OnFinally()
        {
            Transaction?.Dispose();
        }
    }
}
