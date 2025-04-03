using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Student.FnStudent.CASStudentAdvisor;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Student.FnStudent.CASStudentAdvisor
{
    public class GetListCASAdvisorHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;

        public GetListCASAdvisorHandler(
                IStudentDbContext dbContext
            )
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetListCASAdvisorRequest>(
                nameof(GetListCASAdvisorRequest.IdAcademicYear)
                );

            var columns = new[] { "binusianID", "name"};

            var aliasColumns = new Dictionary<string, string>
            {
                { columns[0]   , "UserCAS.Id" },
                { columns[1]   , "UserCAS.DisplayName"}
            };

            var predicate = PredicateBuilder.Create<TrCasAdvisor>(x => x.IdAcademicYear == param.IdAcademicYear);

            if (!string.IsNullOrEmpty(param.Search))
                predicate = predicate.And(x => EF.Functions.Like(x.UserCAS.Id, param.SearchPattern())
                || EF.Functions.Like(x.UserCAS.DisplayName, param.SearchPattern()));

            var query = _dbContext.Entity<TrCasAdvisor>()
                        .Include(x => x.UserCAS)
                        .Include(x => x.TrCasAdvisorStudents)
                        .Where(predicate);

            query = param.OrderBy switch
            {
                "binusianID" => param.OrderType == OrderType.Asc
                        ? query.OrderBy(x => x.UserCAS.Id)
                        : query.OrderByDescending(x => x.UserCAS.Id),
                "name" => param.OrderType == OrderType.Asc
                        ? query.OrderBy(x => x.UserCAS.DisplayName)
                        : query.OrderByDescending(x => x.UserCAS.DisplayName),

                _ => query.OrderByDynamic(param, aliasColumns)
            };

            IReadOnlyList<IItemValueVm> items;
            if (param.Return == CollectionType.Lov)
            {
                items = await query
                    .Select(x => new ItemValueVm
                    {
                        Id = x.Id
                    })
                    .ToListAsync(CancellationToken);
            }
            else
            {
                items =  await query
                   .SetPagination(param)
                   .Select(x => new GetListCASAdvisorResult
                   {
                       Id = x.Id,
                       BinusianID = x.IdUserCAS,
                       FullName = x.UserCAS.DisplayName,
                       IsDelete = x.TrCasAdvisorStudents.Count() > 0 ? false : true
                   })
                   .ToListAsync(CancellationToken);
            }

            var count = param.CanCountWithoutFetchDb(items.Count)
                    ? items.Count
                    : await query.Select(a => a.Id).CountAsync(CancellationToken);

            return Request.CreateApiResult2(items as object, param.CreatePaginationProperty(count).AddColumnProperty(columns));
        }
    }
}
