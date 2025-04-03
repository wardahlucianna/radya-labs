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
using BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.DocumentRequestApprover;
using BinusSchool.Persistence.DocumentDb.Abstractions;
using BinusSchool.Persistence.DocumentDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Document.FnDocument.DocumentRequest.DocumentRequestApprover
{
    public class GetUnmappedApproverStaffListHandler : FunctionsHttpSingleHandler
    {
        //private static readonly string[] _columns = new[] { "IdBinusian", "Name" };
        //private static readonly IDictionary<string, string> _aliasColumns = new Dictionary<string, string>
        //{
        //    { _columns[0], "staff.staff.IdBinusian" },
        //    { _columns[1], "staff.staff.FirstName" }
        //};

        private readonly IDocumentDbContext _dbContext;

        public GetUnmappedApproverStaffListHandler(IDocumentDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetUnmappedApproverStaffListRequest>(
                            nameof(GetUnmappedApproverStaffListRequest.IdSchool));

            var predicate = PredicateBuilder.True<MsStaff>();

            if (!string.IsNullOrWhiteSpace(param.Search))
                predicate = predicate.And(x
                    => EF.Functions.Like(
                        (string.IsNullOrWhiteSpace(x.FirstName) ? "" : (x.FirstName.Trim() + " ")) +
                        (string.IsNullOrWhiteSpace(x.LastName) ? "" : x.LastName.Trim()) 
                        , $"%{param.Search}%")
                    || EF.Functions.Like(x.IdBinusian, param.SearchPattern())
                    );

            //var mappedApproverList = await _dbContext.Entity<MsDocumentReqApprover>()
            //                            .Where(x => x.IdSchool == param.IdSchool)
            //                            .Select(x => x.IdBinusian)
            //                            .ToListAsync(CancellationToken);

            var query = _dbContext.Entity<MsStaff>()
                            .Where(x => x.IdSchool == param.IdSchool)
                            .Where(predicate)
                            .GroupJoin(
                                _dbContext.Entity<MsDocumentReqApprover>()
                                        .Where(x => x.IdSchool == param.IdSchool),
                                staff => staff.IdBinusian,
                                mappedApprover => mappedApprover.IdBinusian,
                                (staff, mappedApprover) => new { staff, mappedApprover }
                            )
                            .SelectMany(
                                x => x.mappedApprover.DefaultIfEmpty(),
                                (staff, mappedApprover) => new { staff, mappedApprover }
                             )
                            .Where(x => x.mappedApprover == null)
                            .OrderBy(x => x.staff.staff.FirstName)
                            .ThenBy(x => x.staff.staff.LastName);
                            //.OrderByDynamic(param, _aliasColumns);

            IReadOnlyList<IItemValueVm> items;
            if (param.Return == CollectionType.Lov)
                items = await query
                    .Select(x => new ItemValueVm
                    {
                        Id = x.staff.staff.IdBinusian,
                        Description = x.staff.staff.IdBinusian + " - " + NameUtil.GenerateFullName(x.staff.staff.FirstName, x.staff.staff.LastName)
                    })
                    .ToListAsync(CancellationToken);
            else
                items = await query
                    .SetPagination(param)
                    .Select(x => new GetUnmappedApproverStaffListResult
                    {
                        Id = x.staff.staff.IdBinusian,
                        Name = NameUtil.GenerateFullName(x.staff.staff.FirstName, x.staff.staff.LastName),
                        BinusianEmail = x.staff.staff.BinusianEmailAddress
                    })
                    .ToListAsync(CancellationToken);

            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : await query.Select(x => x.staff.staff.IdBinusian).CountAsync(CancellationToken);

            //return Request.CreateApiResult2(items as object, param.CreatePaginationProperty(count).AddColumnProperty(_columns));
            return Request.CreateApiResult2(items as object, param.CreatePaginationProperty(count));
        }
    }
}
