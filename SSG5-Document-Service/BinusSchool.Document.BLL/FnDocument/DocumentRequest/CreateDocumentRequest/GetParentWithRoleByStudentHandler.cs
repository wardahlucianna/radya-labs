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
using BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.CreateDocumentRequest;
using BinusSchool.Persistence.DocumentDb.Abstractions;
using BinusSchool.Persistence.DocumentDb.Entities.Student;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Document.FnDocument.DocumentRequest.CreateDocumentRequest
{
    public class GetParentWithRoleByStudentHandler : FunctionsHttpSingleHandler
    {
        private static readonly string[] _columns = new[] { "ParentName", "ParentRole" };
        //private static readonly IDictionary<string, string> _aliasColumns = new Dictionary<string, string>
        //{
        //    { _columns[0], "name" },
        //};

        private readonly IDocumentDbContext _dbContext;

        public GetParentWithRoleByStudentHandler(IDocumentDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetParentWithRoleByStudentRequest>(
                            nameof(GetParentWithRoleByStudentRequest.IdStudent));

            var predicate = PredicateBuilder.True<MsStudentParent>();

            if (!string.IsNullOrWhiteSpace(param.Search))
                predicate = predicate.And(x
                    //=> EF.Functions.Like(x.Name, $"%{param.Search}%")
                    => EF.Functions.Like(x.Parent.FirstName, param.SearchPattern())
                    || EF.Functions.Like(x.Parent.MiddleName, param.SearchPattern())
                    || EF.Functions.Like(x.Parent.LastName, param.SearchPattern())
                    || EF.Functions.Like(x.Parent.ParentRole.ParentRoleNameEng, param.SearchPattern())
                    || EF.Functions.Like(x.Parent.ParentRole.ParentRoleName, param.SearchPattern())
                    );

            var query = _dbContext.Entity<MsStudentParent>()
                            .Include(x => x.Parent)
                                .ThenInclude(x => x.ParentRole)
                            .Where(predicate)
                            .Where(x => x.IdStudent == param.IdStudent);

            query = param.OrderBy switch
            {
                "ParentName" => param.OrderType == OrderType.Asc
                    ? query.OrderBy(x => x.Parent.FirstName).ThenBy(x => x.Parent.MiddleName).ThenBy(x => x.Parent.LastName)
                    : query.OrderByDescending(x => x.Parent.FirstName).ThenByDescending(x => x.Parent.MiddleName).ThenByDescending(x => x.Parent.LastName),
                "ParentRole" => param.OrderType == OrderType.Asc
                    ? query.OrderBy(x => x.Parent.ParentRole.ParentRoleNameEng)
                    : query.OrderByDescending(x => x.Parent.ParentRole.ParentRoleNameEng),
                _ => query.OrderByDynamic(param)
            };

            IReadOnlyList<IItemValueVm> items;
            if (param.Return == CollectionType.Lov)
                items = await query
                    .Select(x => new ItemValueVm
                    {
                        Id = x.Parent.Id,
                        Description = $"{NameUtil.GenerateFullName(x.Parent.FirstName, x.Parent.MiddleName, x.Parent.LastName)} ({x.Parent.ParentRole.ParentRoleNameEng})"
                    })
                    .Distinct()
                    .ToListAsync();
            else
                items = await query
                    .SetPagination(param)
                    .Select(x => new GetParentWithRoleByStudentResult
                    {
                        Id = x.Parent.Id,
                        ParentName = NameUtil.GenerateFullName(x.Parent.FirstName, x.Parent.MiddleName, x.Parent.LastName),
                        ParentRole = new ItemValueVm
                        {
                            Id = x.Parent.ParentRole.Id,
                            Description = x.Parent.ParentRole.ParentRoleNameEng
                        }
                    })
                    .ToListAsync();

            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : await query.Select(x => x.Parent.Id).CountAsync();

            return Request.CreateApiResult2(items as object, param.CreatePaginationProperty(count).AddColumnProperty(_columns));
        }
    }
}
