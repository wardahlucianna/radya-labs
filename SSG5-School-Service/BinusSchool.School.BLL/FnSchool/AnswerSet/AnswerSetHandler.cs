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
using BinusSchool.Data.Model.School.FnSchool.AnswerSet;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.Persistence.SchoolDb.Entities;
using BinusSchool.School.FnSchool.AnswerSet.Validator;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.School.FnSchool.AnswerSet
{
    public class AnswerSetHandler : FunctionsHttpCrudHandler
    {
        private readonly ISchoolDbContext _dbContext;

        public AnswerSetHandler(ISchoolDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> DeleteHandler(IEnumerable<string> ids)
        {
            Transaction = await _dbContext.BeginTransactionAsync(CancellationToken);
            var dataAnswerSet = await _dbContext.Entity<MsAnswerSet>()
                .Where(x => ids.Any(y => y == x.Id))
                .ToListAsync(CancellationToken);

            var undeleted = new UndeletedResult2();

            // find not found ids
            ids = ids.Except(ids.Intersect(dataAnswerSet.Select(x => x.Id)));
            undeleted.NotFound = ids.ToDictionary(x => x, x => string.Format(Localizer["ExNotFound"], x));

            // find already used ids
            foreach (var data in dataAnswerSet)
            {
                data.IsActive = false;
                _dbContext.Entity<MsAnswerSet>().Update(data);
            }

            await _dbContext.SaveChangesAsync(CancellationToken);
            await Transaction.CommitAsync(CancellationToken);

            return Request.CreateApiResult2(errors: undeleted.AsErrors());
        }

        protected override async Task<ApiErrorResult<object>> GetDetailHandler(string id)
        {
            var result = await _dbContext.Entity<MsAnswerSet>()
                                         .Include(x => x.AcademicYear)
                                         .Include(x => x.AnswerSetOptions)
                                         .Where(x => x.Id == id)
                                         .Select(x => new AnswerSetDetailResult
                                         {
                                             Id = x.Id,
                                             AcademicYear = new CodeWithIdVm
                                             {
                                                 Id = x.AcademicYear.Id,
                                                 Code = x.AcademicYear.Code,
                                                 Description = x.AcademicYear.Description
                                             },
                                             AnswerSetName = x.AnswerSetName,
                                             AnswerSetOptions = x.AnswerSetOptions.Any() ? x.AnswerSetOptions.Select(y => new AnswerSetOptionResult
                                             {
                                                 Id = y.Id,
                                                 OptionName = y.AnswerSetOptionName,
                                                 Order = y.Order
                                             }).OrderBy(t => t.Order).ToList() : null
                                         }).SingleOrDefaultAsync();

            if (result is null)
                throw new NotFoundException("AnswerSet is not found");

            return Request.CreateApiResult2(result as object);
        }

        protected override async Task<ApiErrorResult<IReadOnlyList<IItemValueVm>>> GetHandler()
        {
            var param = Request.ValidateParams<GetAnswerSetRequest>(nameof(GetAnswerSetRequest.IdAcademicYear));

            var columns = new[] { "id", "academicYear", "answerSetName"};

            var aliasColumns = new Dictionary<string, string>
            {
                { columns[0], "answerSetName" },
                { columns[1], "academicYear" }
            };

            var predicate = PredicateBuilder.Create<MsAnswerSet>(x => param.IdAcademicYear.Contains(x.IdAcademicYear));
            if (!string.IsNullOrWhiteSpace(param.Search))
                predicate = predicate.And(x
                    => EF.Functions.Like(x.IdAcademicYear, param.SearchPattern())
                       || EF.Functions.Like(x.AnswerSetName, param.SearchPattern()));


            var query = _dbContext.Entity<MsAnswerSet>()
                .Include(x => x.AcademicYear)
                .Where(predicate)
                .OrderByDynamic(param, aliasColumns);

            IReadOnlyList<IItemValueVm> items;
            if (param.Return == CollectionType.Lov)
                items = await query
                    .Select(x => new ItemValueVm(x.Id, x.AnswerSetName))
                    .ToListAsync(CancellationToken);
            else
                items = await query
                    .SetPagination(param)
                    .Select(x => new AnswerSetResult
                    {
                        Id = x.Id,
                        AcademicYear = new CodeWithIdVm
                        {
                            Id = x.AcademicYear.Id,
                            Code = x.AcademicYear.Code,
                            Description = x.AcademicYear.Description
                        },
                        AnswerSetName = x.AnswerSetName
                    })
                    .ToListAsync(CancellationToken);

            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : await query.Select(x => x.Id).CountAsync(CancellationToken);

            return Request.CreateApiResult2(items, param.CreatePaginationProperty(count).AddColumnProperty(columns));
        }

        protected override async Task<ApiErrorResult<object>> PostHandler()
        {
            var body = await Request.ValidateBody<AddAnswerSetRequest, AddAnswerSetValidator>();
            Transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

            var validateData = _dbContext.Entity<MsAnswerSet>()
                                .Where(x => x.IdAcademicYear == body.IdAcademicYear && x.AnswerSetName.Contains(body.AnswerSetName))
                                .FirstOrDefault();
            if(validateData != null)
                throw new BadRequestException($"Answer Set Academic Year : {body.IdAcademicYear} and Name : {body.AnswerSetName} already exists");


            var AnswerSetId = Guid.NewGuid().ToString();
            var answerSet = new MsAnswerSet
            {
                Id = AnswerSetId,
                IdAcademicYear = body.IdAcademicYear,
                AnswerSetName = body.AnswerSetName
            };
            _dbContext.Entity<MsAnswerSet>().Add(answerSet);
            
            var order = 0;
            foreach (var answerSetOption in body.AnswerSetOptions)
            {
                var setOption = new MsAnswerSetOption
                {
                    Id = Guid.NewGuid().ToString(),
                    IdAnswerSet = AnswerSetId,
                    AnswerSetOptionName = answerSetOption.OptionName,
                    Order = order
                };
                order++;
                _dbContext.Entity<MsAnswerSetOption>().Add(setOption);
            }

            await _dbContext.SaveChangesAsync(CancellationToken);
            await Transaction.CommitAsync(CancellationToken);

            return Request.CreateApiResult2();
        }

        protected override async Task<ApiErrorResult<object>> PutHandler()
        {
            var body = await Request.ValidateBody<UpdateAnswerSetRequest, UpdateAnswerSetValidator>();
            Transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

            var data = await _dbContext.Entity<MsAnswerSet>()
                                       .FirstOrDefaultAsync(x => x.Id == body.Id);
            if (data is null)
                throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["AnswerSet"], "Id", body.Id));

            
            data.IdAcademicYear = body.IdAcademicYear;
            data.AnswerSetName = body.AnswerSetName;
            
            _dbContext.Entity<MsAnswerSet>().Update(data);

            var answerSetOptions = _dbContext.Entity<MsAnswerSetOption>().Where(x => x.IdAnswerSet == body.Id).ToList();
            if (answerSetOptions.Any())
            {
                foreach (var answerSetOption in answerSetOptions)
                {
                    answerSetOption.IsActive = false;

                    _dbContext.Entity<MsAnswerSetOption>().Update(answerSetOption);
                }
                _dbContext.Entity<MsAnswerSetOption>().UpdateRange(answerSetOptions);
            }

            foreach (var answerSetOption in body.AnswerSetOptions)
            {
                var setOption = new MsAnswerSetOption
                {
                    Id = Guid.NewGuid().ToString(),
                    IdAnswerSet = data.Id,
                    AnswerSetOptionName = answerSetOption.OptionName
                };

                _dbContext.Entity<MsAnswerSetOption>().Add(setOption);
            }

            await _dbContext.SaveChangesAsync(CancellationToken);
            await Transaction.CommitAsync(CancellationToken);

            return Request.CreateApiResult2();
        }
    }
}
