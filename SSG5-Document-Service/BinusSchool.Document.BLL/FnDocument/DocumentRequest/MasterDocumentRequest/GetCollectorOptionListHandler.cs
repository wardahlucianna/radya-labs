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
using BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.MasterDocumentRequest;
using BinusSchool.Persistence.DocumentDb.Abstractions;
using BinusSchool.Persistence.DocumentDb.Entities.Student;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Document.FnDocument.DocumentRequest.MasterDocumentRequest
{
    public class GetCollectorOptionListHandler : FunctionsHttpSingleHandler
    {
        private static readonly string[] _columns = new[] { "Name", "Role" };

        private readonly IDocumentDbContext _dbContext;

        public GetCollectorOptionListHandler(IDocumentDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetCollectorOptionListRequest>(
                            nameof(GetCollectorOptionListRequest.IdStudent));

            var predicateStudent = PredicateBuilder.True<MsStudent>();
            var predicateParent = PredicateBuilder.True<MsStudentParent>();

            if (!string.IsNullOrWhiteSpace(param.Search))
            {
                predicateParent = predicateParent.And(x
                    //=> EF.Functions.Like(x.Name, $"%{param.Search}%")
                    => EF.Functions.Like(
                                (string.IsNullOrWhiteSpace(x.Parent.FirstName) ? "" : x.Parent.FirstName + " ") +
                                (string.IsNullOrWhiteSpace(x.Parent.MiddleName) ? "" : x.Parent.MiddleName + " ") +
                                (string.IsNullOrWhiteSpace(x.Parent.LastName) ? "" : x.Parent.LastName),
                                param.SearchPattern()
                            )
                    );

                predicateStudent = predicateStudent.And(x
                    //=> EF.Functions.Like(x.Name, $"%{param.Search}%")
                    => EF.Functions.Like(
                                (string.IsNullOrWhiteSpace(x.FirstName) ? "" : x.FirstName + " ") +
                                (string.IsNullOrWhiteSpace(x.MiddleName) ? "" : x.MiddleName + " ") +
                                (string.IsNullOrWhiteSpace(x.LastName) ? "" : x.LastName),
                                param.SearchPattern()
                            )
                    );
            }

            var getStudent = await _dbContext.Entity<MsStudent>()
                                .Where(x => x.Id == param.IdStudent)
                                .Where(predicateStudent)
                                .Select(x => new GetCollectorOptionListResult
                                {
                                    Id = x.Id,
                                    Description = NameUtil.GenerateFullName(x.FirstName, x.MiddleName, x.LastName),
                                    Role = "Student"
                                })
                                .ToListAsync(CancellationToken);

            var getParentList = await _dbContext.Entity<MsStudentParent>()
                                .Include(x => x.Parent)
                                    .ThenInclude(x => x.ParentRole)
                                .Where(predicateParent)
                                .Where(x => x.IdStudent == param.IdStudent)
                                .Select(x => new GetCollectorOptionListResult
                                {
                                    Id = x.IdParent,
                                    Description = NameUtil.GenerateFullName(x.Parent.FirstName, x.Parent.MiddleName, x.Parent.LastName),
                                    Role = x.Parent.ParentRole.ParentRoleNameEng
                                })
                                .ToListAsync(CancellationToken);

            var query = getStudent.Union(getParentList)
                            .AsQueryable();

            query = param.OrderBy switch
            {
                "Name" => param.OrderType == OrderType.Asc
                    ? query.OrderBy(x => x.Description)
                    : query.OrderByDescending(x => x.Description),
                "Role" => param.OrderType == OrderType.Asc
                    ? query.OrderBy(x => x.Role)
                    : query.OrderByDescending(x => x.Role),
                _ => query.OrderByDynamic(param)
            };

            IReadOnlyCollection<IItemValueVm> items;
            if (param.Return == CollectionType.Lov)
                items = query
                            .Select(x => new ItemValueVm
                            {
                                Id = x.Id,
                                Description = x.Description + " (" + x.Role + ")"
                            })
                            .ToList();
            else
                items = query
                            .SetPagination(param)
                            .Select(x => new GetCollectorOptionListResult
                            {
                                Id = x.Id,
                                Description = x.Description,
                                Role = x.Role
                            })
                            .ToList();

            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : await query.Select(x => x).CountAsync();

            return Request.CreateApiResult2(items as object, param.CreatePaginationProperty(count).AddColumnProperty(_columns));
        }
    }
}
