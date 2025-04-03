using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnSchedule.UngenerateSchedule;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Scheduling.FnSchedule.UngenerateSchedule.Validator;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BinusSchool.Scheduling.FnSchedule.UngenerateSchedule
{
    public class UpdateGenerateScheduleStudentHomeroomHandler : FunctionsHttpSingleHandler
    {
        private IDbContextTransaction _transaction;

        private readonly ISchedulingDbContext _dbContext;

        public UpdateGenerateScheduleStudentHomeroomHandler(ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<UpdateGenerateScheduleStudentHomeroomRequest, UpdateGenerateScheduleStudentHomeroomValidator>();

            var dataLesson = await _dbContext.Entity<TrGeneratedScheduleLesson>()
                                                .Include(x => x.GeneratedScheduleStudent)
                                                    .ThenInclude(x => x.GeneratedScheduleGrade)
                                                        .ThenInclude(x => x.GeneratedSchedule)
                                                .Where(x => x.IdHomeroom == body.IdHomeroom)
                                                .Where(x => x.ScheduleDate.Date >= body.Start.Date)
                                                .Where(x => x.ScheduleDate.Date <= body.End.Date)
                                                .Where(x => x.GeneratedScheduleStudent.IdStudent == body.IdStudent)
                                                .Where(x => x.GeneratedScheduleStudent.GeneratedScheduleGrade.IdGrade == body.IdGrade)
                                                .ToListAsync(CancellationToken);
            if (dataLesson.Any())
            {
                // set non generated schedule lesson
                foreach (var dl in dataLesson)
                {
                    dl.IsGenerated = false;
                }
                _dbContext.Entity<TrGeneratedScheduleLesson>().UpdateRange(dataLesson);
            }

            await _dbContext.SaveChangesAsync(CancellationToken);

            return Request.CreateApiResult2();
        }

        protected override Task<IActionResult> OnException(Exception ex)
        {
            _transaction?.Dispose();
            return base.OnException(ex);
        }
    }
}
