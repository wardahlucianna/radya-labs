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
using BinusSchool.Persistence.DocumentDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Document.FnDocument.DocumentRequest.CreateDocumentRequest
{
    public class GetDefaultPICListHandler : FunctionsHttpSingleHandler
    {
        private static readonly string[] _columns = new[] { "IdBinusian", "PICName", "PICEmail" };
        //private static readonly IDictionary<string, string> _aliasColumns = new Dictionary<string, string>
        //{
        //    { _columns[0], "name" },
        //};

        private readonly IDocumentDbContext _dbContext;

        public GetDefaultPICListHandler(IDocumentDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetDefaultPICListRequest>(
                            nameof(GetDefaultPICListRequest.IdDocumentReqType));

            var predicate = PredicateBuilder.True<GetDefaultPICListResult>();
            if (!string.IsNullOrWhiteSpace(param.Search))
                predicate = predicate.And(x
                    => EF.Functions.Like(x.Id, param.SearchPattern())
                    || EF.Functions.Like(x.PICName, param.SearchPattern())
                    || EF.Functions.Like(x.PICEmail, param.SearchPattern())
                    );

            var defaultPICAllTempList = new List<GetDefaultPICListResult>();

            var defaultPICIndividualList = await _dbContext.Entity<MsDocumentReqDefaultPIC>()
                                            .Include(x => x.Staff)
                                            .Where(x => x.IdDocumentReqType == param.IdDocumentReqType)
                                            .Select(x => new GetDefaultPICListResult
                                            {
                                                Id = x.Staff.IdBinusian,
                                                PICName = NameUtil.GenerateFullName(x.Staff.FirstName, x.Staff.LastName),
                                                PICEmail = x.Staff.BinusianEmailAddress
                                            })
                                            .ToListAsync(CancellationToken);

            if(defaultPICIndividualList != null || defaultPICIndividualList.Any())
                defaultPICAllTempList.AddRange(defaultPICIndividualList);

            var defaultPICGroupIdBinusianList = await _dbContext.Entity<MsDocumentReqDefaultPICGroup>()
                                                .Include(x => x.Role)
                                                    .ThenInclude(x => x.UserRoles)
                                                .Where(x => x.IdDocumentReqType == param.IdDocumentReqType)
                                                .Select(x => x.Role.UserRoles.Select(x => x.IdUser).ToList())
                                                .FirstOrDefaultAsync(CancellationToken);

            if (defaultPICGroupIdBinusianList != null)
            {
                if (defaultPICGroupIdBinusianList.Any())
                {
                    var defaultPICGroupList = await _dbContext.Entity<MsStaff>()
                                            .Where(x => defaultPICGroupIdBinusianList.Any(y => y == x.IdBinusian))
                                            .Select(x => new GetDefaultPICListResult
                                            {
                                                Id = x.IdBinusian,
                                                PICName = NameUtil.GenerateFullName(x.FirstName, x.LastName),
                                                PICEmail = x.BinusianEmailAddress
                                            })
                                            .ToListAsync(CancellationToken);

                    defaultPICAllTempList.AddRange(defaultPICGroupList);
                }
            }

            var defaultPICAllQuery = defaultPICAllTempList
                                        .AsQueryable()
                                        .Where(predicate)
                                        .Distinct();

            defaultPICAllQuery = param.OrderBy switch
            {
                "IdBinusian" => param.OrderType == OrderType.Asc
                    ? defaultPICAllQuery.OrderBy(x => x.Id)
                    : defaultPICAllQuery.OrderByDescending(x => x.Id),
                "PICName" => param.OrderType == OrderType.Asc
                    ? defaultPICAllQuery.OrderBy(x => x.PICName)
                    : defaultPICAllQuery.OrderByDescending(x => x.PICName),
                "PICEmail" => param.OrderType == OrderType.Asc
                    ? defaultPICAllQuery.OrderBy(x => x.PICEmail)
                    : defaultPICAllQuery.OrderByDescending(x => x.PICEmail),
                _ => defaultPICAllQuery.OrderByDynamic(param)
            };

            IReadOnlyList<IItemValueVm> items;
            if (param.Return == CollectionType.Lov)
                items = defaultPICAllQuery
                    .Select(x => new ItemValueVm
                    {
                        Id = x.Id,
                        Description = $"{x.Id} - {x.PICName}"
                    })
                    .ToList();
            else
                items = defaultPICAllQuery
                    .SetPagination(param)
                    .Select(x => new GetDefaultPICListResult
                    {
                        Id = x.Id,
                        PICName = x.PICName,
                        PICEmail = x.PICEmail
                    })
                    .ToList();

            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : await defaultPICAllQuery.Select(x => x.Id).CountAsync();

            return Request.CreateApiResult2(items as object, param.CreatePaginationProperty(count).AddColumnProperty(_columns));
        }
    }
}
