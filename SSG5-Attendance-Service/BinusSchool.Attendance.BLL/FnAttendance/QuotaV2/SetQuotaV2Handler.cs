using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Attendance.FnAttendance.QuotaV2.Validator;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Attendance.FnAttendance.QuotaV2;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities;
using BinusSchool.Persistence.AttendanceDb.Entities.School;
using DocumentFormat.OpenXml.Office2010.ExcelAc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;

namespace BinusSchool.Attendance.FnAttendance.QuotaV2
{
    public class SetQuotaV2Handler : FunctionsHttpSingleHandler
    {
        protected IDbContextTransaction Transaction;
        private readonly IAttendanceDbContext _dbContext;

        public SetQuotaV2Handler(IAttendanceDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected async override Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<SetQuotaV2Request, SetQuotaV2Validator>();
            Transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

            var idQuota = Guid.NewGuid().ToString();

            if (!await _dbContext.Entity<MsLevel>()
                                 .AnyAsync(x => x.Id == body.IdLevel))
                throw new NotFoundException("Level is not found");

            var QuotasV2 = await _dbContext.Entity<MsQuota>()
                .Where(x => x.IdLevel == body.IdLevel && x.IdAcademicYear == body.IdAcademicYear)
                .FirstOrDefaultAsync();

            var deletedQuotasDetail = new List<MsQuotaDetail>();

            if (QuotasV2 != null)
            {
                idQuota = QuotasV2.Id;

                deletedQuotasDetail = _dbContext.Entity<MsQuotaDetail>().Where(x => x.IdQuota == idQuota).ToList();
            }
            else
            {
                _dbContext.Entity<MsQuota>().Add(new MsQuota
                {
                    Id = idQuota,
                    IdLevel = body.IdLevel,
                    IdAcademicYear = body.IdAcademicYear
                });
            }

            foreach (var item in body.QuotaDetails)
            {
                var QuotaDetail = deletedQuotasDetail.FirstOrDefault(x => x.AttendanceCategory == item.AttendanceCategory && x.Status == item.Status);
                
                if (QuotaDetail is null)
                    _dbContext.Entity<MsQuotaDetail>().Add(new MsQuotaDetail
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdQuota = idQuota,
                        AttendanceCategory = item.AttendanceCategory,
                        AbsenceCategory = item.AbsenceCategory,
                        ExcusedAbsenceCategory = item.ExcusedAbsenceCategory,
                        Status = item.Status,
                        Percentage = item.Percentage
                    });
                else
                {
                    QuotaDetail.AttendanceCategory = item.AttendanceCategory;
                    QuotaDetail.AbsenceCategory = item.AbsenceCategory;
                    QuotaDetail.ExcusedAbsenceCategory = item.ExcusedAbsenceCategory;
                    QuotaDetail.Status = item.Status;
                    QuotaDetail.Percentage = item.Percentage;
                    _dbContext.Entity<MsQuotaDetail>().Update(QuotaDetail);
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
