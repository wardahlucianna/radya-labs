using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Auth.Authentications.Jwt;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Constants;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Scheduling.FnSchedule.AppointmentBooking;
using BinusSchool.Data.Model.Scheduling.FnSchedule.EmailInvitation;
using BinusSchool.Data.Model.School.FnSchool.SurveyTemplate;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.Persistence.SchoolDb.Entities;
using BinusSchool.School.FnSchool.SurveyTemplate.Validator;
using Microsoft.Azure.WebJobs;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using NPOI.SS.Formula.Functions;

namespace BinusSchool.School.FnSchool.SurveyTemplate
{
    public class SurveyTemplateCopyHandler : FunctionsHttpCrudHandler
    {
        private readonly ISchoolDbContext _dbContext;

        public SurveyTemplateCopyHandler(ISchoolDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override Task<ApiErrorResult<object>> DeleteHandler(IEnumerable<string> ids)
        {
            throw new NotImplementedException();
        }

        protected override Task<ApiErrorResult<object>> GetDetailHandler(string id)
        {
            throw new NotImplementedException();
        }

        protected override async Task<ApiErrorResult<IReadOnlyList<IItemValueVm>>> GetHandler()
        {
            var param = Request.ValidateParams<GetSurveyTemplateCopyRequest>();
            var predicate = PredicateBuilder.Create<MsSurveyTemplate>(x => x.Type == param.Type && x.Status==SurveyTemplateStatus.Done);
            string[] _columns = { "AcademicYear", "TemplateTitle", "Language"};

            if (!string.IsNullOrEmpty(param.IdAcademicYearFrom))
                predicate = predicate.And(x => x.IdAcademicYear == param.IdAcademicYearFrom);

            if (!string.IsNullOrEmpty(param.Search))
                predicate = predicate.And(x => x.Title.Contains(param.Search));

            var listSurveyTemplateToAy = await _dbContext.Entity<MsSurveyTemplate>()
                .Where(e=>e.IdAcademicYear==param.IdAcademicYearTo)
                .Select(x => new
                {
                    Id = x.Id,
                    Title = x.Title,
                }).ToListAsync(CancellationToken);


            var query = _dbContext.Entity<MsSurveyTemplate>()
                .Include(e => e.AcademicYear)
                .Where(predicate)
                .Select(x => new
                {
                    Id = x.Id,
                    AcademicYear = x.AcademicYear.Description,
                    Title = x.Title,
                    Language = x.Language.GetDescription(),
                });

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
            };

            IReadOnlyList<IItemValueVm> items = default;
            if (param.Return == CollectionType.Lov)
            {
                var result = await query
                    .ToListAsync(CancellationToken);


                items = result.Select(x => new GetSurveyTemplateCopyResult
                {
                    Id = x.Id,
                    AcademicYear = x.AcademicYear,
                    Title = x.Title,
                    Language = x.Language,
                    IsCanCopy = listSurveyTemplateToAy.Where(e=>e.Title.ToLower()==x.Title.ToLower()).Any()?false:true
                }).ToList();
            }
            else
            {
                var result = await query
                    .SetPagination(param)
                    .ToListAsync(CancellationToken);

                items = result.Select(x => new GetSurveyTemplateCopyResult
                {
                    Id = x.Id,
                    AcademicYear = x.AcademicYear,
                    Title = x.Title,
                    Language = x.Language,
                    IsCanCopy = listSurveyTemplateToAy.Where(e => e.Title.ToLower() == x.Title.ToLower()).Any() ? false : true
                }).ToList();
            }

            var count = param.CanCountWithoutFetchDb(items.Count)
              ? items.Count
              : query.Select(x => x.Id).Count();

            return Request.CreateApiResult2(items, param.CreatePaginationProperty(count).AddColumnProperty(_columns));
        }

        protected override async Task<ApiErrorResult<object>> PostHandler()
        {
            var body = await Request.ValidateBody<AddSurveyTemplateCopyRequest, AddSurveyTemplateCopyValidator>();
            
            var listSurveyTemplate = await _dbContext.Entity<MsSurveyTemplate>()
               .Where(e => body.ListIdSurveyTemplate.Contains(e.Id))
               .ToListAsync(CancellationToken);

            foreach (var item in listSurveyTemplate)
            {
                var NewSurveyTemplate = new MsSurveyTemplate
                {
                    Id = Guid.NewGuid().ToString(),
                    IdTemplateChild = item.IdTemplateChild,
                    IdAcademicYear = body.IdAcademicYearTo,
                    Language = item.Language,
                    Title = item.Title,
                    Type = item.Type,
                    Status = item.Status,
                };
                _dbContext.Entity<MsSurveyTemplate>().Add(NewSurveyTemplate);
            }
           
            await _dbContext.SaveChangesAsync(CancellationToken);
            return Request.CreateApiResult2();
        }

        protected override Task<ApiErrorResult<object>> PutHandler()
        {
            throw new NotImplementedException();
        }
    }
}
