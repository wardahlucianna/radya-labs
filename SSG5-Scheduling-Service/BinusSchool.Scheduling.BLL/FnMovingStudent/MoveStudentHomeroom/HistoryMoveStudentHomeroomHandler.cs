using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Extensions;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnMovingStudent.MovingStudentHomeroom;
using BinusSchool.Persistence.SchedulingDb.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using BinusSchool.Common.Model.Enums;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Data.Model.Scheduling.FnSchedule.ClassDiary;

namespace BinusSchool.Scheduling.FnMovingStudent.MoveStudentHomeroom
{
    public class HistoryMoveStudentHomeroomHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;

        public HistoryMoveStudentHomeroomHandler(ISchedulingDbContext schoolDbContext)
        {
            _dbContext = schoolDbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<HistoryMoveStudentHomeroomRequest>();

            string[] _columns = { "HomeroomNew", "HomeroomOld", "EffectiveDate", "Note"};

            var query = _dbContext.Entity<HTrMoveStudentHomeroom>()
                            .Include(e => e.HomeroomOld).ThenInclude(e => e.Grade)
                            .Include(e => e.HomeroomOld).ThenInclude(e => e.GradePathwayClassroom).ThenInclude(e => e.Classroom)
                            .Include(e => e.HomeroomNew).ThenInclude(e => e.Grade)
                            .Include(e => e.HomeroomNew).ThenInclude(e => e.GradePathwayClassroom).ThenInclude(e => e.Classroom)
                            .Where(e => e.IdHomeroomStudent == param.IdHomeroomStudent && e.IsShowHistory)
                            .Select(e => new 
                            {
                                Id = e.IdHTrMoveStudentHomeroom,
                                HomeroomNew = e.HomeroomNew.Grade.Code + e.HomeroomNew.GradePathwayClassroom.Classroom.Code,
                                HomeroomOld = e.HomeroomOld.Grade.Code + e.HomeroomOld.GradePathwayClassroom.Classroom.Code,
                                EffectiveDate = e.StartDate,
                                Note = e.Note
                            });

            switch (param.OrderBy)
            {
                case "HomeroomNew":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.HomeroomNew)
                        : query.OrderBy(x => x.HomeroomNew);
                    break;
                case "HomeroomOld":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.HomeroomOld)
                        : query.OrderBy(x => x.HomeroomOld);
                    break;
                case "EffectiveDate":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.EffectiveDate)
                        : query.OrderBy(x => x.EffectiveDate);
                    break;
            };

            IReadOnlyList<IItemValueVm> items = default;
            if (param.Return == CollectionType.Lov)
            {
                var result = await query
                    .ToListAsync(CancellationToken);

                items = result
                        .Select(e => new HistoryMoveStudentHomeroomResult
                        {
                            HomeroomNew = e.HomeroomNew,
                            HomeroomOld = e.HomeroomOld,
                            EffectiveDate = e.EffectiveDate,
                            Note = e.Note
                        }).ToList();
            }
            else
            {
                var result = await query
                    .SetPagination(param)
                    .ToListAsync(CancellationToken);

                items = result
                        .Select(e => new HistoryMoveStudentHomeroomResult
                        {
                            HomeroomNew = e.HomeroomNew,
                            HomeroomOld = e.HomeroomOld,
                            EffectiveDate = e.EffectiveDate,
                            Note = e.Note
                        }).ToList();
            }

            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : await query.Select(x => x.Id).CountAsync(CancellationToken);

            return Request.CreateApiResult2(items as object, param.CreatePaginationProperty(count).AddColumnProperty(_columns));
        }
    }
}
