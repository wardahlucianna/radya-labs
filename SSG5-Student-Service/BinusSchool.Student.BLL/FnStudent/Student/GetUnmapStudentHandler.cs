using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model;
using BinusSchool.Data.Model.Student.FnStudent.Student;
using BinusSchool.Domain.Extensions;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Student.FnStudent.Student
{
    public class GetUnmapStudentHandler : FunctionsHttpSingleHandler
    {
        private static readonly Lazy<string[]> _columns = new Lazy<string[]>(new[] { "id", "fullName", "email" });
        private static readonly Lazy<IDictionary<string, string>> _aliasColumns = new Lazy<IDictionary<string, string>>(new Dictionary<string, string>
        {
            { _columns.Value[1], "firstName" },
            { _columns.Value[2], "binusianEmailAddress" }
        });

        private readonly IStudentDbContext _dbContext;

        public GetUnmapStudentHandler(IStudentDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.GetParams<CollectionSchoolRequest>();
            var predicate = PredicateBuilder.Create<MsStudent>(x => !string.IsNullOrEmpty(x.BinusianEmailAddress));

            if (!string.IsNullOrWhiteSpace(param.Search))
                predicate = predicate.And(x
                    => EF.Functions.Like(x.Id, param.SearchPattern())
                    //|| EF.Functions.Like(x.FirstName, param.SearchPattern())
                    //|| EF.Functions.Like(x.MiddleName, param.SearchPattern())
                    //|| EF.Functions.Like(x.LastName, param.SearchPattern())
                    || EF.Functions.Like((string.IsNullOrWhiteSpace(x.FirstName) ? "" : x.FirstName.Trim() + " ") +
                                        (string.IsNullOrWhiteSpace(x.MiddleName) ? "" : x.MiddleName.Trim() + " ") +
                                        (string.IsNullOrWhiteSpace(x.LastName) ? "" : x.LastName.Trim()), param.SearchPattern()
                                        )
                    || EF.Functions.Like(x.BinusianEmailAddress, param.SearchPattern()));

            if (param.IdSchool != null && param.IdSchool.Any())
                predicate = predicate.And(x => param.IdSchool.Contains(x.IdSchool));

            // default order by first name
            param.OrderBy ??= _columns.Value[1];

            var query = _dbContext.Entity<MsStudent>()
                .SearchByIds(param)
                .Where(predicate)
                .OrderByDynamic(param, _aliasColumns.Value);

            IReadOnlyList<IItemValueVm> items;
            if (param.Return == CollectionType.Lov)
                items = await query
                    .Select(x => new ItemValueVm(x.Id, x.FirstName))
                    .ToListAsync(CancellationToken);
            else
                items = await query
                    .SetPagination(param)
                    .Select(x => new GetUnmapStudentResult
                    {
                        Id = x.Id,
                        FullName = NameUtil.GenerateFullName(x.FirstName, x.MiddleName, x.LastName),
                        Email = x.BinusianEmailAddress
                    })
                    .ToListAsync(CancellationToken);

            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : await query.Select(x => x.Id).CountAsync(CancellationToken);

            return Request.CreateApiResult2(items as object, param.CreatePaginationProperty(count).AddColumnProperty(_columns.Value));
        }
    }
}
