using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model;
using BinusSchool.Data.Model.School.FnSchool.AcademicYear;
using BinusSchool.Data.Model.School.FnSchool.AnswerSet;
using BinusSchool.Data.Model.School.FnSchool.School;
using BinusSchool.Data.Model.Student.FnStudent.StudentDemographicsReport;
using BinusSchool.Domain.Extensions;
using BinusSchool.Persistence.Extensions;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.Persistence.SchoolDb.Entities;
using BinusSchool.School.FnSchool.AcademicYear.Validator;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.School.FnSchool.AcademicYear
{
    public class AcademicYearHandler : FunctionsHttpCrudHandler
    {
        private readonly ISchoolDbContext _dbContext;

        public AcademicYearHandler(ISchoolDbContext schoolDbContext)
        {
            _dbContext = schoolDbContext;
        }

        protected override async Task<ApiErrorResult<object>> DeleteHandler(IEnumerable<string> ids)
        {
            var idAcademicYear = ids.FirstOrDefault();

            var dataAy = await _dbContext.Entity<MsAcademicYear>()
                .Where(x => x.Id == idAcademicYear)
            .FirstOrDefaultAsync(CancellationToken);

            if (dataAy is null)
                throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["AcademicYear"], "Id", idAcademicYear));

            int cekDataacademicYearDataUsed = await ValidateAyHasUseAsync(idAcademicYear);

            if (cekDataacademicYearDataUsed == 1)
                throw new BadRequestException($"Academic Year data with Code {idAcademicYear} cannot be Remove, because it has already been used");

            dataAy.IsActive = false;
            _dbContext.Entity<MsAcademicYear>().Update(dataAy);

            await _dbContext.SaveChangesAsync(CancellationToken);

            return Request.CreateApiResult2();
        }

        protected override async Task<ApiErrorResult<object>> GetDetailHandler(string id)
        {
            var data = await _dbContext.Entity<MsAcademicYear>()
               .Include(x => x.School)
               .Select(x => new DetailResult2
               {
                   Id = x.Id,
                   Code = x.Code,
                   Description = x.Description,
                   School = new GetSchoolResult
                   {
                       Id = x.School.Id,
                       Name = x.School.Name,
                       Description = x.School.Description
                   },
                   Audit = x.GetRawAuditResult2()
               })
               .FirstOrDefaultAsync(x => x.Id == id);

            return Request.CreateApiResult2(data as object);
        }

        protected override async Task<ApiErrorResult<IReadOnlyList<IItemValueVm>>> GetHandler()
        {
            var param = Request.ValidateParams<CollectionSchoolRequest>(nameof(CollectionSchoolRequest.IdSchool));
            var columns = new[] { "code", "description" };

            var predicate = PredicateBuilder.Create<MsAcademicYear>(x => param.IdSchool.Any(y => y == x.IdSchool));
            if (!string.IsNullOrWhiteSpace(param.Search))
                predicate = predicate.And(x
                    => EF.Functions.Like(x.Code, $"%{param.Search}%")
                    || EF.Functions.Like(x.Description, $"%{param.Search}%"));

            var query = _dbContext.Entity<MsAcademicYear>()
                .Include(x => x.Levels)
                .Include(x => x.Pathways)
                .Include(x => x.Departments)
                .Include(x => x.SanctionMappings)
                .Include(x => x.TextbookUserPeriods)
                .Include(x => x.TextbookSubjectGroups)
                .Include(x => x.Textbooks)
                .Include(x => x.AnswerSets)
                .Include(x => x.SurveyTemplates)
                .Include(x => x.PublishSurveys)
                .Include(x => x.Homerooms)
                .Include(x => x.Lessons)
                .Include(x => x.ScheduleRealization2s)
                .SearchByIds(param)
                .Where(predicate)
                .Where(x => param.IdSchool.Any(y => y == x.IdSchool));

            switch (param.OrderBy)
            {
                case "description":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.Description)
                        : query.OrderBy(x => x.Description);
                    break;

                case "code":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.Code)
                        : query.OrderBy(x => x.Code);
                    break;

                default:
                    query = query.OrderByDescending(x => x.Code);
                    break;
            };

            IReadOnlyList<IItemValueVm> items;
            if (param.Return == CollectionType.Lov)
                items = await query
                    .Select(x => new ItemValueVm(x.Id, x.Description))
                    .ToListAsync(CancellationToken);
            else
                items = await query
                    .SetPagination(param)
                    .Select(x => new GetListAnswerSetResult
                    {
                        Id = x.Id,
                        Code = x.Code,
                        Description = x.Description,
                        IsCanDeletedOrEdit = (x.Levels.FirstOrDefault().Id != null || x.Pathways.FirstOrDefault().Id != null || x.Departments.FirstOrDefault().Id != null || x.SanctionMappings.FirstOrDefault().Id != null || x.TextbookUserPeriods.FirstOrDefault().Id != null ||
                                                x.TextbookSubjectGroups.FirstOrDefault().Id != null || x.Textbooks.FirstOrDefault().Id != null || x.AnswerSets.FirstOrDefault().Id != null || x.SurveyTemplates.FirstOrDefault().Id != null || x.PublishSurveys.FirstOrDefault().Id != null ||
                                                x.Homerooms.FirstOrDefault().Id != null || x.Lessons.FirstOrDefault().Id != null || x.ScheduleRealization2s.FirstOrDefault().Id != null) ? false : true
                    })
                    .ToListAsync(CancellationToken);

            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : await query.Select(x => x.Id).CountAsync(CancellationToken);

            return Request.CreateApiResult2(items, param.CreatePaginationProperty(count).AddColumnProperty(columns));
        }

        protected override async Task<ApiErrorResult<object>> PostHandler()
        {
            var body = await Request.ValidateBody<AddAcademicYearRequest, AddAcademicYearValidator>();

            var dataAcademicYear = await _dbContext.Entity<MsAcademicYear>().Where(x => x.Code == body.Code && x.IdSchool == body.IdSchool).FirstOrDefaultAsync();
            if (dataAcademicYear != null)
                throw new BadRequestException(string.Format(Localizer["ExAlreadyExist"], Localizer["Academic Year"], "Code", body.Code));

            var getDataSchool = _dbContext.Entity<MsSchool>().Where(x => x.Id == body.IdSchool).FirstOrDefault();
            if (getDataSchool == null)
                throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["School"], "Id", body.IdSchool));

            var idAcademicYear = string.Concat(getDataSchool.Name.ToUpper(), body.Code);

            var dataIdAcademicYear = await _dbContext.Entity<MsAcademicYear>().Where(x => x.Id == idAcademicYear).IgnoreQueryFilters().FirstOrDefaultAsync();
            if (dataIdAcademicYear == null)
            {
                var param = new MsAcademicYear
                {
                    Id = idAcademicYear,
                    Code = body.Code,
                    Description = body.Description,
                    IdSchool = body.IdSchool
                };

                _dbContext.Entity<MsAcademicYear>().Add(param);
            }
            else
            {
                dataIdAcademicYear.Code = body.Code;
                dataIdAcademicYear.Description = body.Description;
                dataIdAcademicYear.IdSchool = body.IdSchool;
                dataIdAcademicYear.IsActive = true;

                _dbContext.Entity<MsAcademicYear>().Update(dataIdAcademicYear);
            }

            await _dbContext.SaveChangesAsync(CancellationToken);

            return Request.CreateApiResult2();
        }

        protected override async Task<ApiErrorResult<object>> PutHandler()
        {
            var body = await Request.ValidateBody<UpdateAcademicYearRequest, UpdateAcademicYearValidator>();

            var dataAy = await _dbContext.Entity<MsAcademicYear>().FindAsync(body.Id);
            if (dataAy is null)
                throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["AcademicYear"], "Id", body.Id));

            var dataSchool = await _dbContext.Entity<MsSchool>().FindAsync(body.IdSchool);
            if (dataSchool is null)
                throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["School"], "Id", body.IdSchool));

            if (dataAy.Code != body.Code)
            {
                var validateDuploCodeAy = await _dbContext.Entity<MsAcademicYear>().Where(x => x.Code == body.Code && x.IdSchool == body.IdSchool && x.Id != body.Id).FirstOrDefaultAsync();
                if (validateDuploCodeAy != null)
                    throw new BadRequestException(string.Format(Localizer["ExAlreadyExist"], Localizer["AcademicYear"], "Code", body.Code));
            }

            int cekDataacademicYearDataUsed = await ValidateAyHasUseAsync(body.Id);

            if (cekDataacademicYearDataUsed == 1)
                throw new BadRequestException($"Academic Year data with Code {body.Id} cannot be changed, because it has already been used");

            dataAy.Code = body.Code;
            dataAy.Description = body.Description;
            dataAy.IdSchool = body.IdSchool;

            _dbContext.Entity<MsAcademicYear>().Update(dataAy);

            await _dbContext.SaveChangesAsync(CancellationToken);

            return Request.CreateApiResult2();
        }

        private async Task<int> ValidateAyHasUseAsync(string id)
        {
            var academicYearDataUsed = await _dbContext.Entity<MsAcademicYear>()
                .Include(x => x.Levels)
                .Include(x => x.Pathways)
                .Include(x => x.Departments)
                .Include(x => x.SanctionMappings)
                .Include(x => x.TextbookUserPeriods)
                .Include(x => x.TextbookSubjectGroups)
                .Include(x => x.Textbooks)
                .Include(x => x.AnswerSets)
                .Include(x => x.SurveyTemplates)
                .Include(x => x.PublishSurveys)
                .Include(x => x.Homerooms)
                .Include(x => x.Lessons)
                .Include(x => x.ScheduleRealization2s)
                .Where(x => x.Id == id)
                .Select(x => new
                {
                    IdAy = x.Id,
                    IdLevels = x.Levels.FirstOrDefault().Id,
                    IdPathways = x.Pathways.FirstOrDefault().Id,
                    IdDepartments = x.Departments.FirstOrDefault().Id,
                    IdSanctionMappings = x.SanctionMappings.FirstOrDefault().Id,
                    IdTextbookUserPeriods = x.TextbookUserPeriods.FirstOrDefault().Id,
                    IdTextbookSubjectGroups = x.TextbookSubjectGroups.FirstOrDefault().Id,
                    IdTextbooks = x.Textbooks.FirstOrDefault().Id,
                    IdAnswerSets = x.AnswerSets.FirstOrDefault().Id,
                    IdSurveyTemplates = x.SurveyTemplates.FirstOrDefault().Id,
                    IdPublishSurveys = x.PublishSurveys.FirstOrDefault().Id,
                    IdHomerooms = x.Homerooms.FirstOrDefault().Id,
                    IdLessons = x.Lessons.FirstOrDefault().Id,
                    IdScheduleRealization2s = x.ScheduleRealization2s.FirstOrDefault().Id,
                })
                .FirstOrDefaultAsync(CancellationToken);

            if (academicYearDataUsed.IdLevels != null || academicYearDataUsed.IdPathways != null || academicYearDataUsed.IdDepartments != null || academicYearDataUsed.IdSanctionMappings != null || academicYearDataUsed.IdTextbookUserPeriods != null ||
                academicYearDataUsed.IdTextbookSubjectGroups != null || academicYearDataUsed.IdTextbooks != null || academicYearDataUsed.IdAnswerSets != null || academicYearDataUsed.IdSurveyTemplates != null || academicYearDataUsed.IdPublishSurveys != null ||
                academicYearDataUsed.IdHomerooms != null || academicYearDataUsed.IdLessons != null || academicYearDataUsed.IdScheduleRealization2s != null)
                return 1;
            else
                return 0;
        }
    }
}
