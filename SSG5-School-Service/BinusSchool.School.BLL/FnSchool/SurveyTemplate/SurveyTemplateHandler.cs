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
using BinusSchool.Data.Model.School.FnSchool.SurveyTemplate;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.Persistence.SchoolDb.Entities;
using BinusSchool.School.FnSchool.SurveyTemplate.Validator;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.School.FnSchool.SurveyTemplate
{
    public class SurveyTemplateHandler : FunctionsHttpCrudHandler
    {
        private readonly ISchoolDbContext _dbContext;

        public SurveyTemplateHandler(ISchoolDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> DeleteHandler(IEnumerable<string> ids)
        {
            var getTemplateSurvey = await _dbContext.Entity<MsSurveyTemplate>()
              .Where(x => ids.Contains(x.Id))
              .ToListAsync(CancellationToken);

            getTemplateSurvey.ForEach(x => x.IsActive = false);
            _dbContext.Entity<MsSurveyTemplate>().UpdateRange(getTemplateSurvey);
            await _dbContext.SaveChangesAsync(CancellationToken);
            return Request.CreateApiResult2();
        }

        protected override async Task<ApiErrorResult<object>> GetDetailHandler(string id)
        {
            var item = await _dbContext.Entity<MsSurveyTemplate>()
               .Include(e => e.AcademicYear)
               .Where(e=>e.Id==id)
               .Select(x => new DetailSurveyTemplateResult
               {
                   Id = x.Id,
                   AcademicYear = new ItemValueVm
                   {
                       Id = x.AcademicYear.Id,
                       Description = x.AcademicYear.Description,
                   },
                   Title = x.Title,
                   LanguageEnum = x.Language,
                   Language = x.Language.GetDescription()
               }).ToListAsync(CancellationToken);

            return Request.CreateApiResult2(item as object);
        }

        protected override async Task<ApiErrorResult<IReadOnlyList<IItemValueVm>>> GetHandler()
        {
            var param = Request.ValidateParams<GetSurveyTemplateRequest>();
            var predicate = PredicateBuilder.Create<MsSurveyTemplate>(x => x.Type==param.Type);
            string[] _columns = { "AcademicYear", "TemplateTitle", "Language", "LastUpdate", "Status"};

            if (!string.IsNullOrEmpty(param.IdAcademicYear))
                predicate = predicate.And(x => x.IdAcademicYear == param.IdAcademicYear);

            if (!string.IsNullOrEmpty(param.Search))
                predicate = predicate.And(x => x.Title.Contains(param.Search));

            var listSurveyTemplate = await _dbContext.Entity<MsSurveyTemplate>()
                .Include(e=>e.AcademicYear)
                .Include(e=>e.PublishSurveys)
                .Where(predicate)
                .Select(x => new
                {
                    Id = x.Id,
                    IdTemplateChild = x.IdTemplateChild,
                    AcademicYear = x.AcademicYear.Description,
                    Title = x.Title,
                    Language = x.Language.GetDescription(),
                    Status = x.Status.GetDescription(),
                    IsDelete = x.PublishSurveys.Any()?false:true,
                    LastUpdate = x.DateUp==null?x.DateIn:x.DateUp,
                }).ToListAsync(CancellationToken);

            var query = listSurveyTemplate.Distinct();
            //ordering
            switch (param.OrderBy)
            {
                case "AcademicYear":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.AcademicYear)
                        : query.OrderBy(x => x.AcademicYear);
                    break;

                case "TemplateTitle":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.Title)
                        : query.OrderBy(x => x.Title);
                    break;
                case "Language":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.Language)
                        : query.OrderBy(x => x.Language);
                    break;
                case "Status":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.Status)
                        : query.OrderBy(x => x.Status);
                    break;
                case "LastUpdate":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.LastUpdate)
                        : query.OrderBy(x => x.LastUpdate);
                    break;
            };

            IReadOnlyList<IItemValueVm> items = default;
            if (param.Return == CollectionType.Lov)
            {
                var result = query
                    .ToList();


                items = result.Select(x => new GetSurveyTemplateResult
                {
                    Id = x.Id,
                    AcademicYear = x.AcademicYear,
                    Title = x.Title,
                    Language = x.Language,
                    Status = x.Status,
                    IdTemplateChild = x.IdTemplateChild,
                    IsDelete = x.IsDelete,
                    LastUpdate = x.LastUpdate
                }).ToList();
            }
            else
            {
                var result = query
                    .SetPagination(param)
                    .ToList();

                items = result.Select(x => new GetSurveyTemplateResult
                {
                    Id = x.Id,
                    AcademicYear = x.AcademicYear,
                    Title = x.Title,
                    Language = x.Language,
                    Status = x.Status,
                    IdTemplateChild = x.IdTemplateChild,
                    IsDelete = x.IsDelete,
                    LastUpdate = x.LastUpdate
                }).ToList();
            }

            var count = param.CanCountWithoutFetchDb(items.Count)
              ? items.Count
              : query.Select(x => x.Id).Count();

            return Request.CreateApiResult2(items, param.CreatePaginationProperty(count).AddColumnProperty(_columns));
        }

        protected override async Task<ApiErrorResult<object>> PostHandler()
        {
            var body = await Request.ValidateBody<AddSurveyTemplateRequest, AddSurveyTemplateValidator>();

            var getSurveyTemplate = await _dbContext.Entity<MsSurveyTemplate>()
                                        .Where(e => e.Title == body.Title && e.Type==body.Type)
                                        .FirstOrDefaultAsync(CancellationToken);

            if(getSurveyTemplate!=null)
                throw new BadRequestException("Title survey template is exsis");

            var NewSurveyTemplate = new MsSurveyTemplate
            {
                Id = body.Id,
                IdTemplateChild = body.IdTemplateChild,
                IdAcademicYear = body.IdAcademicYear,
                Language = body.Language,
                Title = body.Title,
                Type = body.Type,
                Status = body.Status,
            };
            _dbContext.Entity<MsSurveyTemplate>().Add(NewSurveyTemplate);
            await _dbContext.SaveChangesAsync(CancellationToken);
            return Request.CreateApiResult2();
        }

        protected override async Task<ApiErrorResult<object>> PutHandler()
        {
            var body = await Request.ValidateBody<UpdateSurveyTemplateRequest, UpdateSurveyTemplateValidator>();

            var getSurveyTemplate = await _dbContext.Entity<MsSurveyTemplate>()
                                        .Where(e => e.Id==body.Id)
                                        .FirstOrDefaultAsync(CancellationToken);

            if(getSurveyTemplate==null)
                throw new BadRequestException("survey template is not found");


            getSurveyTemplate.Title = body.Title;
            getSurveyTemplate.IdTemplateChild = body.IdTemplateChild;
            getSurveyTemplate.Language = body.Language;
            getSurveyTemplate.Status = body.Status;

            _dbContext.Entity<MsSurveyTemplate>().Update(getSurveyTemplate);
            await _dbContext.SaveChangesAsync(CancellationToken);

            return Request.CreateApiResult2();
        }
    }
}
