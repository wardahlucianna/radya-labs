using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.School.FnSchool.MeritDemerit;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.Persistence.SchoolDb.Entities;
using BinusSchool.School.FnSchool.MeritDemerit.Validator;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.School.FnSchool.MeritDemerit
{
    public class ScoreContinuationSettingHeandler : FunctionsHttpCrudHandler
    {
        private readonly ISchoolDbContext _dbContext;

        public ScoreContinuationSettingHeandler(ISchoolDbContext schoolDbContext)
        {
            _dbContext = schoolDbContext;
        }

        protected override async Task<ApiErrorResult<object>> DeleteHandler(IEnumerable<string> ids)
        {
            
            return Request.CreateApiResult2();
        }

        protected override async Task<ApiErrorResult<object>> GetDetailHandler(string id)
        {
            return Request.CreateApiResult2();
        }

        protected override async Task<ApiErrorResult<IReadOnlyList<IItemValueVm>>> GetHandler()
        {
            var param = Request.ValidateParams<GetScoreContinuationSettingRequest>();
            var predicate = PredicateBuilder.Create<MsGrade>(x => x.IsActive == true);
            string[] _columns = { "AcademicYear", "Level", "Grade", "Option", "Score", "Every", "Operation" };

            if (!string.IsNullOrEmpty(param.IdAcademiYear))
                predicate = predicate.And(x => x.Level.IdAcademicYear == param.IdAcademiYear);
            if (!string.IsNullOrEmpty(param.IdLevel))
                predicate = predicate.And(x => x.IdLevel == param.IdLevel);
            if (!string.IsNullOrEmpty(param.IdGrade))
                predicate = predicate.And(x => x.Id == param.IdGrade);

            var query = _dbContext.Entity<MsGrade>()
                .Where(predicate)
               .Select(x => new 
               {
                   Id = x.Id,
                   AcademicYear = x.Level.AcademicYear.Description,
                   IdAcademicYear = x.Level.IdAcademicYear,
                   Level = x.Level.Description,
                   Grade = x.Description,
                   IsPointSystem = _dbContext.Entity<MsMeritDemeritComponentSetting>().Any(e => e.IdGrade == x.Id) ? _dbContext.Entity<MsMeritDemeritComponentSetting>().SingleOrDefault(e => e.IdGrade == x.Id).IsUsePointSystem : true,
                   Score = _dbContext.Entity<MsScoreContinuationSetting>().Any(e => e.IdGrade == x.Id && e.Category== param.Category) ? _dbContext.Entity<MsScoreContinuationSetting>().Distinct().SingleOrDefault(e => e.IdGrade == x.Id && e.Category == param.Category).Score : 0,
                   Option = _dbContext.Entity<MsScoreContinuationSetting>().Any(e => e.IdGrade == x.Id && e.Category == param.Category) ? _dbContext.Entity<MsScoreContinuationSetting>().Distinct().SingleOrDefault(e => e.IdGrade == x.Id && e.Category == param.Category).ScoreContinueOption.ToString() : "",
                   Every = _dbContext.Entity<MsScoreContinuationSetting>().Any(e => e.IdGrade == x.Id && e.Category == param.Category) ? _dbContext.Entity<MsScoreContinuationSetting>().Distinct().SingleOrDefault(e => e.IdGrade == x.Id && e.Category == param.Category).ScoreContinueEvery.ToString() : "",
                   Operation = _dbContext.Entity<MsScoreContinuationSetting>().Any(e => e.IdGrade == x.Id && e.Category == param.Category) ? _dbContext.Entity<MsScoreContinuationSetting>().Distinct().SingleOrDefault(e => e.IdGrade == x.Id && e.Category == param.Category).Operation.ToString() : "",
               }); 

            //ordering
            switch (param.OrderBy)
            {
                case "AcademicYear":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.AcademicYear)
                        : query.OrderBy(x => x.AcademicYear);
                    break;
                case "Level":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.Level)
                        : query.OrderBy(x => x.Level);
                    break;
                case "Grade":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.Grade)
                        : query.OrderBy(x => x.Grade);
                    break;
                case "Option":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.Option)
                        : query.OrderBy(x => x.Option);
                    break;
                case "Score":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.Score)
                        : query.OrderBy(x => x.Score);
                    break;
                case "Every":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.Every)
                        : query.OrderBy(x => x.Every);
                    break;
            };

            IReadOnlyList<IItemValueVm> items;

            if (param.Return == CollectionType.Lov)
            {
                var result = await query
                    .ToListAsync(CancellationToken);

                items = result.Select(x => new GetScoreContinuationSettingResult
                {
                    IdGrade = x.Id,
                    AcademicYear = x.AcademicYear,
                    IdAcademicYear = x.Id,
                    Level = x.Level,
                    Grade = x.Grade,
                    IsPointSystem = x.IsPointSystem,
                    Score = x.Score,
                    Option = new ItemValueVm
                    {
                        Id = x.Option,
                        Description = x.Option
                    },
                    Every = new ItemValueVm
                    {
                        Id = x.Every,
                        Description = x.Every
                    },
                    Operation = new ItemValueVm
                    {
                        Id = (x.Operation != "Null") ? x.Operation : null,
                        Description = (x.Operation != "Null") ? x.Operation : null
                    }
                }).ToList();
            }
            else
            {
                var result = await query
                    .SetPagination(param)
                    .ToListAsync(CancellationToken);
                items = result.Select(x => new GetScoreContinuationSettingResult
                {
                    IdGrade = x.Id,
                    AcademicYear = x.AcademicYear,
                    IdAcademicYear = x.Id,
                    Level = x.Level,
                    Grade = x.Grade,
                    IsPointSystem = x.IsPointSystem,
                    Score = x.Score,
                    Option = new ItemValueVm
                    {
                        Id = x.Option,
                        Description = x.Option
                    },
                    Every = new ItemValueVm
                    {
                        Id = x.Every,
                        Description = x.Every
                    },
                    Operation = new ItemValueVm
                    {
                        Id = (x.Operation != "Null") ? x.Operation : null,
                        Description = (x.Operation != "Null") ? x.Operation : null
                    }
                }).ToList();
            }

            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : await query.Select(x => x.Id).CountAsync(CancellationToken);

            return Request.CreateApiResult2(items, param.CreatePaginationProperty(count).AddColumnProperty(_columns));
        }

        protected override async Task<ApiErrorResult<object>> PostHandler()
        {
            var body = await Request.ValidateBody<UpdateScoreContinuationSettingRequest, UpdateScoreContinuationSettingValidator>();
            var ScoreContinuation = _dbContext.Entity<MsScoreContinuationSetting>().Where(e => e.Grade.Level.IdAcademicYear == body.IdAcademicYear).ToList();

            if (ScoreContinuation == null)
                throw new BadRequestException("Score continuation with academic year: " + body.IdAcademicYear + " is not found.");

            foreach (var ItemScoreContinuation in body.ScoreContinuationSettings)
            {
                var ScoreContinuationByGrade = ScoreContinuation.SingleOrDefault(e => e.IdGrade == ItemScoreContinuation.IdGrade && e.Category == ItemScoreContinuation.Category);
                if (ScoreContinuationByGrade == null)
                {
                    var newScoreContinuation = new MsScoreContinuationSetting
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdGrade = ItemScoreContinuation.IdGrade,
                        Category = ItemScoreContinuation.Category,
                        ScoreContinueOption = ItemScoreContinuation.Option,
                        Score = ItemScoreContinuation.Score,
                        ScoreContinueEvery = ItemScoreContinuation.Every,
                        Operation = ItemScoreContinuation.Operation
                    };
                    _dbContext.Entity<MsScoreContinuationSetting>().Add(newScoreContinuation);
                }
                else
                {
                    ScoreContinuationByGrade.IdGrade = ItemScoreContinuation.IdGrade;
                    ScoreContinuationByGrade.ScoreContinueOption = ItemScoreContinuation.Option;
                    ScoreContinuationByGrade.Score = ItemScoreContinuation.Score;
                    ScoreContinuationByGrade.ScoreContinueEvery = ItemScoreContinuation.Every;
                    ScoreContinuationByGrade.Category = ItemScoreContinuation.Category;
                    ScoreContinuationByGrade.Operation = ItemScoreContinuation.Operation;

                    _dbContext.Entity<MsScoreContinuationSetting>().Update(ScoreContinuationByGrade);
                }
            }
            await _dbContext.SaveChangesAsync(CancellationToken);
            return Request.CreateApiResult2();
        }

        protected override async Task<ApiErrorResult<object>> PutHandler()
        {
            var param = Request.ValidateParams<GetScoreContinuationSettingRequest>();
            var ScoreContinuation = await _dbContext.Entity<MsScoreContinuationSetting>().Where(e => e.Grade.Level.IdAcademicYear == param.IdAcademiYear).ToListAsync(CancellationToken);

            if(ScoreContinuation.Any())
                throw new BadRequestException("Score continuation with academic year: " + param.IdAcademiYear + " is not found.");

            ScoreContinuation.ForEach(x => x.IsActive = false);
            _dbContext.Entity<MsScoreContinuationSetting>().UpdateRange(ScoreContinuation);
            await _dbContext.SaveChangesAsync(CancellationToken);
            return Request.CreateApiResult2();
        }
    }
}
