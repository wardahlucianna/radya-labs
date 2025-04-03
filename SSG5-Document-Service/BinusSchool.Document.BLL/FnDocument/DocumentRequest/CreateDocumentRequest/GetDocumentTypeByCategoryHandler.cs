using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.CreateDocumentRequest;
using BinusSchool.Persistence.DocumentDb.Abstractions;
using BinusSchool.Persistence.DocumentDb.Entities;
using BinusSchool.Persistence.DocumentDb.Entities.School;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Document.FnDocument.DocumentRequest.CreateDocumentRequest
{
    public class GetDocumentTypeByCategoryHandler : FunctionsHttpSingleHandler
    {
        private static readonly string[] _columns = new[] { "IdDocumentReqType", "DocumentName" };
        //private static readonly IDictionary<string, string> _aliasColumns = new Dictionary<string, string>
        //{
        //    { _columns[0], "id" },
        //    { _columns[1], "name" },
        //};

        private readonly IDocumentDbContext _dbContext;

        public GetDocumentTypeByCategoryHandler(IDocumentDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetDocumentTypeByCategoryRequest>(
                            nameof(GetDocumentTypeByCategoryRequest.IdSchool),
                            nameof(GetDocumentTypeByCategoryRequest.IsAcademicDocument),
                            nameof(GetDocumentTypeByCategoryRequest.RequestByParent));

            var predicate = PredicateBuilder.True<MsDocumentReqType>();
            if (!string.IsNullOrWhiteSpace(param.Search))
                predicate = predicate.And(x
                    => EF.Functions.Like(x.Name, param.SearchPattern())
                    );

            if (param.IsAcademicDocument && string.IsNullOrEmpty(param.IdGrade))
                throw new BadRequestException("Please choose at least one academic year / grade");

            var getGradeAY = await _dbContext.Entity<MsGrade>()
                                    .Include(x => x.Level)
                                        .ThenInclude(x => x.AcademicYear)
                                    .Where(x => x.Id == param.IdGrade)
                                    .Select(x => !param.IsAcademicDocument ? null : new
                                    {
                                        AcademicYear = new CodeWithIdVm
                                        {
                                            Id = x.Level.AcademicYear.Id,
                                            Code = x.Level.AcademicYear.Code,
                                            Description = x.Level.AcademicYear.Description
                                        },
                                        Grade = new CodeWithIdVm
                                        {
                                            Id = x.Id,
                                            Code = x.Code,
                                            Description = x.Description
                                        }
                                    })
                                    .FirstOrDefaultAsync(CancellationToken);

            var query = _dbContext.Entity<MsDocumentReqType>()
                            .Include(x => x.StartAcademicYear)
                            .Include(x => x.EndAcademicYear)
                            .Include(x => x.DocumentReqTypeGradeMappings)
                        .Where(predicate)
                        .Where(x => x.IdSchool == param.IdSchool &&
                                    x.Status == true &&
                                    x.IsAcademicDocument == param.IsAcademicDocument &&

                                    // condition for request by parent
                                    (!param.RequestByParent ? true : x.VisibleToParent) &&

                                    // condition for academic document
                                    (!param.IsAcademicDocument ? true :
                                        (getGradeAY == null ? false :
                                            (x.DocumentReqTypeGradeMappings.Where(y => y.CodeGrade == getGradeAY.Grade.Code).Any() &&
                                                (string.IsNullOrEmpty(x.StartAcademicYear.Code) ? true : x.StartAcademicYear.Code.CompareTo(getGradeAY.AcademicYear.Code) <= 0) &&
                                                (string.IsNullOrEmpty(x.EndAcademicYear.Code) ? true : x.EndAcademicYear.Code.CompareTo(getGradeAY.AcademicYear.Code) >= 0)
                                            )
                                        )
                                    )
                                );

            query = param.OrderBy switch
            {
                "IdDocumentReqType" => param.OrderType == OrderType.Asc
                    ? query.OrderBy(x => x.Id)
                    : query.OrderByDescending(x => x.Id),
                "DocumentName" => param.OrderType == OrderType.Asc
                    ? query.OrderBy(x => x.Name)
                    : query.OrderByDescending(x => x.Name),
                _ => query.OrderByDynamic(param)
            };

            IReadOnlyList<IItemValueVm> items;
            if (param.Return == CollectionType.Lov)
                items = await query
                    .Select(x => new ItemValueVm
                    {
                        Id = x.Id,
                        Description = x.Name
                    })
                    .ToListAsync(CancellationToken);
            else
                items = await query
                    .SetPagination(param)
                    .Select(x => new GetDocumentTypeByCategoryResult
                    {
                        Id = x.Id,
                        DocumentName = x.Name,
                        Description = x.Description
                    })
                    .ToListAsync(CancellationToken);

            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : await query.Select(x => x.Id).CountAsync();

            return Request.CreateApiResult2(items as object, param.CreatePaginationProperty(count).AddColumnProperty(_columns));
        }
    }
}
