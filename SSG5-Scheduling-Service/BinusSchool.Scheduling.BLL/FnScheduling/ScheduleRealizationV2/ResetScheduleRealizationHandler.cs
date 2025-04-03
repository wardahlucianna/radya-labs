using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using Microsoft.EntityFrameworkCore;
using BinusSchool.Data.Model.Scheduling.FnSchedule.ScheduleRealizationV2;
using BinusSchool.Scheduling.FnSchedule.ScheduleRealizationV2.Validator;
using BinusSchool.Common.Exceptions;

namespace BinusSchool.Scheduling.FnSchedule.ScheduleRealizationV2
{
    public class ResetScheduleRealizationHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;

        public ResetScheduleRealizationHandler(ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<ResetScheduleRealizationRequest, ResetScheduleRealizationV2Validator>();

            foreach (var dataBody in body.DataScheduleRealizations)
            {
                var trScheduleRealization = _trScheduleRealization(dataBody).Result;

                if (trScheduleRealization == null)
                {
                    throw new BadRequestException("No any changes data schedule realization to reset");
                }

                //inActive status history
                var existScheduleRealizationHistory = await _htrScheduleRealization(dataBody);
                existScheduleRealizationHistory.ForEach(e => e.IsActive = false);

                //inActive status schedule realization
                trScheduleRealization.IsActive = false;

                _dbContext.Entity<HTrScheduleRealization2>().UpdateRange(existScheduleRealizationHistory);
                _dbContext.Entity<TrScheduleRealization2>().Update(trScheduleRealization);
            }

            await _dbContext.SaveChangesAsync(CancellationToken);

            return Request.CreateApiResult2();
        }

        private async Task<TrScheduleRealization2> _trScheduleRealization(ResetDataScheduleRealizationV2 dataBody)
        {
            var data = await _dbContext.Entity<TrScheduleRealization2>()
                    .Where(x => x.ScheduleDate == dataBody.Date
                                && x.IdBinusian == dataBody.IdUserTeacher
                                && x.ClassID == dataBody.ClassID
                                && x.SessionID == dataBody.SessionID
                                && x.IdLesson == dataBody.IdLesson
                                && x.IdAcademicYear == dataBody.IdAcademicYear
                                && x.IdLevel == dataBody.IdLevel
                                && x.IdGrade == dataBody.IdGrade
                                && x.IdDay == dataBody.IdDay)
                    .OrderByDescending(x => x.DateIn)
                    .FirstOrDefaultAsync(CancellationToken);

            return data;
        }

        private async Task<List<HTrScheduleRealization2>> _htrScheduleRealization(ResetDataScheduleRealizationV2 dataBody)
        {
            var data = await _dbContext.Entity<HTrScheduleRealization2>()
                    .Where(x => x.ScheduleDate == dataBody.Date
                                && x.IdBinusian == dataBody.IdUserTeacher
                                && x.ClassID == dataBody.ClassID
                                && x.SessionID == dataBody.SessionID
                                && x.IdLesson == dataBody.IdLesson
                                && x.IdAcademicYear == dataBody.IdAcademicYear
                                && x.IdLevel == dataBody.IdLevel
                                && x.IdGrade == dataBody.IdGrade
                                && x.IdDay == dataBody.IdDay)
                    .OrderByDescending(x => x.DateIn)
                    .ToListAsync(CancellationToken);

            return data;
        }
    }
}
