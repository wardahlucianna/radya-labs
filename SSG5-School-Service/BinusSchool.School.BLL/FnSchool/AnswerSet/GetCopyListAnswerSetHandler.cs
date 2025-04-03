using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.School.FnSchool.AnswerSet;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.Persistence.SchoolDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.School.FnSchool.AnswerSet
{
    public class GetCopyListAnswerSetHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchoolDbContext _dbContext;

        public GetCopyListAnswerSetHandler(ISchoolDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetCopyListAnswerSetRequest>(nameof(GetCopyListAnswerSetRequest.IdAcademicYear),
                                                                            nameof(GetCopyListAnswerSetRequest.CopyToIdAcademicYear));

            var columns = new[] { "id", "academicYear", "answerSetName" };

            var aliasColumns = new Dictionary<string, string>
            {
                { columns[0], "answerSetName" },
                { columns[1], "academicYear" }
            };

            var predicate = PredicateBuilder.Create<MsAnswerSet>(x => param.IdAcademicYear == x.IdAcademicYear);
            if (!string.IsNullOrWhiteSpace(param.Search))
                predicate = predicate.And(x
                    => EF.Functions.Like(x.IdAcademicYear, param.SearchPattern())
                       || EF.Functions.Like(x.AnswerSetName, param.SearchPattern()));


            var query = _dbContext.Entity<MsAnswerSet>()
                .Include(x => x.AcademicYear)
                .Where(predicate)
                .OrderByDynamic(param, aliasColumns)
                .Select(x => new GetCopyListAnswerSetHandlerResult
                {
                    Id = x.Id,
                    AcademicYear = new CodeWithIdVm()
                    {
                        Id = x.AcademicYear.Id,
                        Code = x.AcademicYear.Code,
                        Description = x.AcademicYear.Description
                    },
                    AnswerSetName = x.AnswerSetName,
                    IsCanCopied = true
                });

            var getDataNewAnswerSet = _dbContext.Entity<MsAnswerSet>().Where(x => x.IdAcademicYear == param.CopyToIdAcademicYear).ToList();

            if (getDataNewAnswerSet.Any())
            {
                var OldAsName = query.Select(x => x.AnswerSetName).ToList();
                var NewAsName = getDataNewAnswerSet.Select(x => x.AnswerSetName).ToList();

                var oldNotInNewAS = OldAsName.Except(NewAsName).ToList();

                query = query
                            .Select(x => new GetCopyListAnswerSetHandlerResult
                            {
                                Id = x.Id,
                                AcademicYear = new CodeWithIdVm()
                                {
                                    Id = x.AcademicYear.Id,
                                    Code = x.AcademicYear.Code,
                                    Description = x.AcademicYear.Description
                                },
                                AnswerSetName = x.AnswerSetName,
                                IsCanCopied = (oldNotInNewAS.Any(y => y == x.AnswerSetName)) ? true : false
                            });
            }

            var res = await query.SetPagination(param).ToListAsync(CancellationToken);

            var count = param.CanCountWithoutFetchDb(res.Count)
            ? res.Count
          : await query.CountAsync(CancellationToken);

            return Request.CreateApiResult2(res as object, param.CreatePaginationProperty(count).AddColumnProperty(columns));
        }
    }
}
